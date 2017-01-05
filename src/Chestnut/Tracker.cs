using System;
using System.Collections.Generic;
using System.Linq;
using Chestnut.Packets;
using Chestnut.Util;
using Chestnut.Torrent;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using StackExchange.Redis;
using System.Text;
using System.Threading;

namespace Chestnut
{
    public enum RunState
    {
        Started,
        Stopped
    }
    public class Tracker
    {
        public MemCache RedisBacking;
        public uint AnnounceInterval = 10 * 60;
        public int ListenPort;
        public RunState TrackerState = RunState.Stopped;


        private static TaskScheduler taskScheduler;

        public Tracker(int port)
        {
            try
            {
                RedisBacking = new MemCache(StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379"));
            }
            catch(RedisConnectionException e)
            { Console.WriteLine("Redis must be running!"); }
                

            ListenPort = port;

            taskScheduler = new TaskScheduler();

            //taskScheduler.Add(TimeSpan.FromSeconds(5), () =>
            taskScheduler.Add(TimeSpan.FromSeconds(AnnounceInterval + 15), () =>
            {
                RedisBacking.PurgeAllOldPeers(TimeSpan.FromSeconds(AnnounceInterval + 15));
            });

            Thread schedulerThread = new Thread(SchedulerLoop)
            {
                IsBackground = true
            };
            schedulerThread.Start();

            //RedisBacking.AddPeer(new TorrentPeer("127.0.0.1", 1), new byte[] { 1, 2, 3 }, DateTime.Now - TimeSpan.FromSeconds(10), PeerType.Seeder);
            //RedisBacking.AddPeer(new TorrentPeer("127.0.0.1", 2), new byte[] { 1, 2, 3 }, DateTime.Now - TimeSpan.FromSeconds(25), PeerType.Seeder);
            //RedisBacking.AddPeer(new TorrentPeer("127.0.0.1", 3), new byte[] { 1, 2, 3 }, DateTime.Now - TimeSpan.FromSeconds(30), PeerType.Seeder);
            //RedisBacking.AddPeer(new TorrentPeer("127.0.0.1", 4), new byte[] { 1, 2, 3 }, DateTime.Now - TimeSpan.FromSeconds(200), PeerType.Seeder);
            //RedisBacking.AddPeer(new TorrentPeer("127.0.0.1", 5), new byte[] { 1, 2, 3 }, DateTime.Now - TimeSpan.FromSeconds(300), PeerType.Seeder);
            //RedisBacking.AddPeer(new TorrentPeer("127.0.0.1", 6), new byte[] { 1, 2, 3 }, DateTime.Now - TimeSpan.FromSeconds(400), PeerType.Seeder);
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

            if (receivedData.Length > 8)
            {
                var action = Unpack.UInt32(receivedData, 8);

                switch ((TorrentAction)action)
                {
                    case TorrentAction.Connect:
                        var connectRequest = new ConnectRequest(receivedData);
                        Console.WriteLine("[Connect] from " + addressString + ":" + res.RemoteEndPoint.Port);

                        var connectResponse = new ConnectResponse(0, connectRequest.TransactionID, (long)13376969);
                        SendDataAsync(client, connectResponse.Data, res.RemoteEndPoint);
                        break;


                    case TorrentAction.Announce:
                        var announceRequest = new AnnounceRequest(receivedData);
                        Console.WriteLine("[Announce] from " + addressString + ":" + announceRequest.Port + ", " + (TorrentEvent)announceRequest.TorrentEvent);

                        var peer = new TorrentPeer(addressString, announceRequest.Port);

                        if ((TorrentEvent)announceRequest.TorrentEvent != TorrentEvent.Stopped)
                            RedisBacking.AddPeer(peer, announceRequest.InfoHash, DateTime.Now);
                        else
                            RedisBacking.RemovePeer(peer, announceRequest.InfoHash);

                        var peers = RedisBacking.GetPeers(announceRequest.InfoHash);
                        var torrentInfo = RedisBacking.ScrapeHash(announceRequest.InfoHash);

                        var announceResponse = new AnnounceResponse(announceRequest.TransactionID, AnnounceInterval, torrentInfo.Leechers, torrentInfo.Seeders, peers);
                        SendDataAsync(client, announceResponse.Data, res.RemoteEndPoint);
                        break;

                    case TorrentAction.Scrape:
                        var scrapeRequest = new ScrapeRequest(receivedData);
                        Console.WriteLine(string.Format("[Scrape] from {0} for {1} torrents", addressString, scrapeRequest.InfoHashes.Count));

                        var scrapedTorrents = RedisBacking.ScrapeHashes(scrapeRequest.InfoHashes);

                        var scrapeResponse = new ScrapeResponse(scrapeRequest.TransactionID, scrapedTorrents);
                        SendDataAsync(client, scrapeResponse.Data, res.RemoteEndPoint);
                        break;
                    default:
                        Console.WriteLine("Unknown Data: " + Encoding.UTF8.GetString(receivedData));
                        break;
                }
            }
            else
            {
                Console.WriteLine("Unknown data: " + Encoding.UTF8.GetString(receivedData));
            }
        }

        public void SchedulerLoop()
        {
            while (true)
            {
                taskScheduler.Run();
                Thread.Sleep(10);
            }
        }

        public static async void SendDataAsync(UdpClient client, byte[] data, IPEndPoint endpoint)
        {
            await client.SendAsync(data, data.Length, endpoint);
        }
    }
}
