using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using Tracker.Packets;
using Tracker.Util;
using Tracker.Torrent;
using StackExchange.Redis;

namespace Tracker
{
    public class Program
    {
        static List<byte[]> ConnectionRequests = new List<byte[]>();
        static List<byte[]> TransactionIDs = new List<byte[]>();
        static int connectionIndex = 0;
        public static ConnectionMultiplexer RedisConnection = ConnectionMultiplexer.Connect("localhost");

        static void Main(string[] args)
        {
            //AddPeer(new TorrentPeer("127.0.0.1", 6969),new byte[] { 0, 1 });
            //GetPeers(new byte[] { 0, 1 });
            Task.Run(async () =>
            {
                var localEndpoint = new IPEndPoint(IPAddress.Any, 27775);
                using (var udpClient = new UdpClient(localEndpoint))
                {
                    while (true)
                    {
                        var receivedResults = await udpClient.ReceiveAsync();
                        DoShit(receivedResults, udpClient);
                    }
                }
            });

            Console.ReadLine();
        }

        public static void ArrayCopy(ref byte[] source, int sourceIndex,ref byte[] destination)
        {
            Array.Copy(source, sourceIndex, destination, 0, destination.Length);
        }

        public static void AddPeer(TorrentPeer peer, byte[] hash)
        {
            var db = RedisConnection.GetDatabase();
            string insert = peer.StringPeer();
            db.SetAdd("torrent:" + Unpack.Hex(hash), insert);
        }

        public static void RemovePeer(TorrentPeer peer, byte[] hash)
        {
            var db = RedisConnection.GetDatabase();
            string insert = peer.StringPeer();
            db.SetRemove("torrent:" + Unpack.Hex(hash), insert);
        }

        public static List<TorrentPeer> GetPeers(byte[] hash)
        {
            var peers = new List<TorrentPeer>();
            var db = RedisConnection.GetDatabase();
            var value = db.SetMembers("torrent:" + Unpack.Hex(hash));

            foreach(var peer in value)
            {
                if(peer.HasValue)
                {
                    var peerResponse = (string)peer;
                    peers.Add(new TorrentPeer(peerResponse));
                }
            }
            return peers;
        }

        public static List<TorrentInfo> ScrapeHashes(List<byte[]> hashes)
        {
            List<TorrentInfo> list = new List<TorrentInfo>();
            foreach(var hash in hashes)
            {
                list.Add(new TorrentInfo(hash, 1, 1, 0)); //oops
            }
            return list;
        }

        public static void DoShit(UdpReceiveResult res, UdpClient client)
        {
            var receivedData = res.Buffer;
            var endPointAddress = res.RemoteEndPoint.Address;
            var addressString = (endPointAddress.ToString() == "127.0.0.1") ? "192.168.1.105" : endPointAddress.ToString();

            if (receivedData.Length > 12)
            {
                var action = Unpack.UInt32(receivedData, 8);
                switch(action)
                {
                    case 0:
                        var connectRequest = new ConnectRequest(receivedData);
                        Console.WriteLine("Connect from " + addressString + ":" + res.RemoteEndPoint.Port);

                        var connectResponse = new ConnectResponse(0, connectRequest.TransactionID, (long)13376969);
                        SendDataAsync(client,connectResponse.Data,  res.RemoteEndPoint);
                        break;


                    case 1:
                        var announceRequest = new AnnounceRequest(receivedData);
                        //var address = endPointAddress.ToString();

                        var peer = new TorrentPeer(addressString, (ushort)announceRequest.Port);
                        Console.WriteLine("Announce from " + addressString + ":" + announceRequest.Port + ", " + (TorrentEvent)announceRequest.TorrentEvent);
                        Console.WriteLine(announceRequest.Downloaded + "," + announceRequest.Uploaded + "," + announceRequest.Left);
                        if ((TorrentEvent)announceRequest.TorrentEvent != TorrentEvent.Stopped)
                            AddPeer(peer, announceRequest.InfoHash);
                        else
                            RemovePeer(peer, announceRequest.InfoHash);

                        var peers = GetPeers(announceRequest.InfoHash);
                        var announceResponse = new AnnounceResponse(announceRequest.TransactionID, 30, 1, (uint)4, peers);
                        SendDataAsync(client, announceResponse.Data, res.RemoteEndPoint);
                        break;


                    case 2:
                        var scrapeRequest = new ScrapeRequest(receivedData);
                        Console.WriteLine(string.Format("Scrape request from {0} for {1} torrents", addressString, scrapeRequest.InfoHashes.Count));

                        var scrapedTorrents = ScrapeHashes(scrapeRequest.InfoHashes);
                        var scrapeResponse = new ScrapeResponse(scrapeRequest.TransactionID, scrapedTorrents);

                        SendDataAsync(client, scrapeResponse.Data, res.RemoteEndPoint);
                        
                        break;
                    default:
                        Console.WriteLine(Encoding.UTF8.GetString(receivedData));
                        break;
                }
            }
            else
            {
                Console.WriteLine(Encoding.UTF8.GetString(receivedData));
            }
        }

        public static async void SendDataAsync(UdpClient client, byte[] data, IPEndPoint endpoint)
        {
            //Console.WriteLine(DateTime.Now.ToString() + " [Response to " + endpoint.Address.ToString() +"]\r\n");
            await client.SendAsync(data, data.Length, endpoint);
        }
    }
}