using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;

namespace Tracker.Torrent
{
    public class TorrentInfo
    {
        public byte[] InfoHash;
        public UInt32 Seeders;
        public UInt32 Completed;
        public UInt32 Leechers;

        public List<TorrentPeer> Peers;

        public TorrentInfo(byte[] hash, uint seeders, uint completed, uint leechers)
        {
            InfoHash = hash;
            Seeders = seeders;
            Completed = completed;
            Leechers = leechers;
        }

        public byte[] PackedTorrentInfo()
        {
            return Pack.UInt32(Seeders).
                Concat(Pack.UInt32(Completed)).
                Concat(Pack.UInt32(Leechers)).ToArray();
        }
    }
}
