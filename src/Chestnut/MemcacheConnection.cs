using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Chestnut.Packets;
using Chestnut.Util;
using Chestnut.Torrent;

namespace Chestnut
{
    public class MemCache
    {
        public ConnectionMultiplexer RedisBacking;
        public MemCache(ConnectionMultiplexer redis)
        {
            RedisBacking = redis;
        }

        public void AddPeer(TorrentPeer peer, byte[] hash, DateTime time, PeerType type = PeerType.Seeder)
        {
            var db = RedisBacking.GetDatabase();

            string peerString = peer.StringPeer();
            var stringHash = Unpack.Hex(hash);

            var firstAdd = db.SetAdd("torrents", stringHash); //keep list of all torrents being managed
            if (firstAdd)
                Console.WriteLine("Registering " + hash);

            db.SortedSetAdd("t:" + stringHash, peerString, UtilityFunctions.GetUnixTimestamp(time));

            if (type == PeerType.Seeder)
                db.HashIncrement("p:" + stringHash, "seeders");
            else
                db.HashIncrement("p:" + stringHash, "leechers");
        }

        public void RemovePeer(TorrentPeer peer, byte[] hash, PeerType type = PeerType.Seeder)
        {
            RemovePeer(peer, Unpack.Hex(hash), type);
        }


        public void RemovePeer(TorrentPeer peer, string hashString, PeerType type = PeerType.Seeder)
        {
            var db = RedisBacking.GetDatabase();

            string peerString = peer.StringPeer();
            
            db.SortedSetRemove("t:" + hashString, peerString);

            var cardinality = db.SortedSetLength("t:" + hashString);
            if (cardinality == 0)
            {
                db.SetRemove("torrents", hashString);
                Console.WriteLine("Deregistering " + hashString);
            }
                
            Console.WriteLine("Removed " + peerString + " from " + hashString + "," + cardinality + " left");

            var fields = db.HashGetAll("p:" + hashString);
            foreach (var field in fields)
            {
                var value = int.Parse(field.Value);

                if (field.Name == "seeders" && value > 0 && type == PeerType.Seeder)
                    db.HashDecrement("p:" + hashString, "seeders");

                if (field.Name == "leechers" && value > 0 && type == PeerType.Leecher)
                    db.HashDecrement("p:" + hashString, "leechers");

            }

        }

        public List<TorrentPeer> GetPeers(byte[] hash, int maxNum = 100)
        {
            var db = RedisBacking.GetDatabase();

            var hashString = Unpack.Hex(hash);
            var peers = new List<TorrentPeer>();

            //TODO: choose random subset of peerslist
            var value = db.SortedSetRangeByRank("t:" + Unpack.Hex(hash), 0, maxNum, Order.Descending);

            foreach (var peer in value)
            {
                if (peer.HasValue)
                {
                    var peerResponse = (string)peer;
                    peers.Add(new TorrentPeer(peerResponse));
                }
            }
            return peers;
        }

        public TorrentInfo ScrapeHash(byte[] hash)
        {
            return ScrapeHashes(new List<byte[]> { hash }).First();
        }

        public int PurgeAllOldPeers(TimeSpan timeCutoff)
        {
            var db = RedisBacking.GetDatabase();

            var torrents = db.SetMembers("torrents");
            foreach (var hash in torrents)
            {
                var dateCutoff = UtilityFunctions.GetUnixTimestamp(DateTime.Now - timeCutoff);

                if (hash.HasValue)
                {
                    string torrentHash = System.Text.Encoding.ASCII.GetString((byte[])hash);
                    var peersToDelete = db.SortedSetRangeByScore("t:" + torrentHash, double.NegativeInfinity, dateCutoff);

                    foreach(var peer in peersToDelete)
                    {
                        if (peer.HasValue)
                        {
                            var peerString = (string)peer;
                            RemovePeer(new TorrentPeer(peerString), torrentHash);
                        }
                    }
                }
            }

            return 0;
        }


        public List<TorrentInfo> ScrapeHashes(List<byte[]> hashes)
        {
            List<TorrentInfo> list = new List<TorrentInfo>();
            var db = RedisBacking.GetDatabase();
            foreach (var hash in hashes)
            {
                var entries = db.HashGetAll("p:" + hash);
                uint seeders = 0;
                uint leechers = 0;

                foreach(var entry in entries)
                {
                    if (entry.Name == "seeders")
                        seeders = uint.Parse(entry.Value);
                    else
                        leechers = uint.Parse(entry.Value);
                }
                
                list.Add(new TorrentInfo(hash, seeders, seeders, leechers));
            }
            return list;
        }
    }
}
