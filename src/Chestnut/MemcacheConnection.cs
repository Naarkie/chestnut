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

        public void ResetHash(string hash)
        {
            var db = RedisBacking.GetDatabase();
            var nullCheck = db.StringGet("s:" + hash);
            if (nullCheck.IsNullOrEmpty)
            {
                db.StringSet("s:" + hash, "0");
                db.StringSet("l:" + hash, "0");
            }
        }

        public void AddPeer(TorrentPeer peer, byte[] hash, PeerType type = PeerType.Seeder)
        {
            var db = RedisBacking.GetDatabase();
            string insert = peer.StringPeer();
            var stringHash = Unpack.Hex(hash);

            //ResetHash(stringHash);

            db.SetAdd("t:" + stringHash, insert);


            if (type == PeerType.Seeder)
                db.StringIncrement("s:" + stringHash); //amount of seeders
            else
                db.StringIncrement("l:" + stringHash); //amount of leechers
        }


        public void RemovePeer(TorrentPeer peer, byte[] hash, PeerType type = PeerType.Seeder)
        {
            var db = RedisBacking.GetDatabase();
            string insert = peer.StringPeer();
            var stringHash = Unpack.Hex(hash);

            //ResetHash(stringHash);

            db.SetRemove("t:" + Unpack.Hex(hash), insert);

            if (type == PeerType.Seeder)
                db.StringDecrement("s:" + Unpack.Hex(hash));
            else
                db.StringDecrement("l:" + Unpack.Hex(hash));
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

        public List<TorrentInfo> ScrapeHashes(List<byte[]> hashes)
        {
            List<TorrentInfo> list = new List<TorrentInfo>();
            var db = RedisBacking.GetDatabase();
            foreach (var hash in hashes)
            {
                var seeders = db.StringGet("s:" + hash);
                var leechers = db.StringGet("l:" + hash);

                if (seeders.IsNullOrEmpty)
                    seeders = 0;

                if (leechers.IsNullOrEmpty)
                    leechers = 0;
                
                list.Add(new TorrentInfo(hash, uint.Parse(seeders), uint.Parse(seeders), uint.Parse(leechers)));
            }
            return list;
        }
    }
}
