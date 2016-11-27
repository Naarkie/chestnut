using System;
using System.Collections.Generic;
using System.Linq;
using Tracker.Packets;
using Tracker.Util;
using Tracker.Torrent;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Tracker
{
    public enum RunState
    {
        Started,
        Stopped
    }
    public class Tracker
    {
        public MemCache RedisBacking;
        public uint AnnounceInterval = 60;
        public int ListenPort;
        public RunState TrackerState = RunState.Stopped;

        public Tracker(int port)
        {
            RedisBacking = new MemCache(StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379"));
            ListenPort = port;
            
        }

        public void Start()
        {
            TrackerState = RunState.Started;
            Task.Run(async () =>
            {
                var localEndpoint = new IPEndPoint(IPAddress.Any, ListenPort);
                var localIP = localEndpoint.Address.ToString();
                using (var udpClient = new UdpClient(localEndpoint))
                {
                    Console.WriteLine($"Listening on {localIP}:{ListenPort}\n");
                    while (!false)
                    {
                        var receivedResults = await udpClient.ReceiveAsync();
                        ReceivedData(receivedResults, udpClient);
                    }
                }
            });
        }




        public void ReceivedData(UdpReceiveResult res, UdpClient client)
        {
            var receivedData = res.Buffer;
            var endPointAddress = res.RemoteEndPoint.Address;
            var addressString = endPointAddress.ToString();

            if (receivedData.Length > 12)
            {
                var action = Unpack.UInt32(receivedData, 8); //connect,announce,scrape
                switch (action)
                {
                    case 0:
                        var connectRequest = new ConnectRequest(receivedData);
                        Console.WriteLine("[Connect] from " + addressString + ":" + res.RemoteEndPoint.Port);

                        var connectResponse = new ConnectResponse(0, connectRequest.TransactionID, (long)13376969);
                        SendDataAsync(client, connectResponse.Data, res.RemoteEndPoint);
                        break;


                    case 1:
                        var announceRequest = new AnnounceRequest(receivedData);
                        Console.WriteLine("[Announce] from " + addressString + ":" + announceRequest.Port + ", " + (TorrentEvent)announceRequest.TorrentEvent);

                        var peer = new TorrentPeer(addressString, announceRequest.Port);

                        if ((TorrentEvent)announceRequest.TorrentEvent != TorrentEvent.Stopped)
                            RedisBacking.AddPeer(peer, announceRequest.InfoHash);
                        else
                            RedisBacking.RemovePeer(peer, announceRequest.InfoHash);

                        var peers = RedisBacking.GetPeers(announceRequest.InfoHash);
                        var torrentInfo = RedisBacking.ScrapeHashes(new List<byte[]>() { announceRequest.InfoHash });
                        var announceResponse = new AnnounceResponse(announceRequest.TransactionID, AnnounceInterval, torrentInfo.First().Leechers, torrentInfo.First().Seeders, peers);
                        SendDataAsync(client, announceResponse.Data, res.RemoteEndPoint);
                        break;


                    case 2:
                        var scrapeRequest = new ScrapeRequest(receivedData);
                        Console.WriteLine(string.Format("[Scrape] from {0} for {1} torrents", addressString, scrapeRequest.InfoHashes.Count));

                        var scrapedTorrents = RedisBacking.ScrapeHashes(scrapeRequest.InfoHashes);
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
            await client.SendAsync(data, data.Length, endpoint);
        }
    }
}
