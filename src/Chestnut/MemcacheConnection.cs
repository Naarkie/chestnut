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

        public void AddPeer(TorrentPeer peer, byte[] hash, PeerType type = PeerType.Seeder)
        {
            var db = RedisBacking.GetDatabase();
            string insert = peer.StringPeer();
            var stringHash = Unpack.Hex(hash);

            db.SetAdd("t:" + stringHash, insert);

            if (type == PeerType.Seeder)
                db.HashIncrement("p:" + stringHash, "seeders");
            else
                db.HashIncrement("p:" + stringHash, "leechers");
        }


        public void RemovePeer(TorrentPeer peer, byte[] hash, PeerType type = PeerType.Seeder)
        {
            var db = RedisBacking.GetDatabase();
            string insert = peer.StringPeer();
            var stringHash = Unpack.Hex(hash);

            db.SetRemove("t:" + stringHash, insert);

            var fields = db.HashGetAll("p:" + stringHash);
            foreach (var field in fields)
            {
                var value = int.Parse(field.Value);

                if(field.Name == "seeders" && value > 0 && type == PeerType.Seeder)
                    db.HashDecrement("p:" + stringHash, "seeders");

                if (field.Name == "leechers" && value > 0 && type == PeerType.Leecher)
                    db.HashDecrement("p:" + stringHash, "leechers");

            }
        }

        public List<TorrentPeer> GetPeers(byte[] hash)
        {
            var peers = new List<TorrentPeer>();
            var db = RedisBacking.GetDatabase();
            var value = db.SetMembers("t:" + Unpack.Hex(hash));

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
