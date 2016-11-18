using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;
using Tracker.Torrent;
using System.Net;

namespace Tracker.Packets
{
    public class AnnounceResponse
    {
        public byte[] Data;

        public UInt32 Action;
        public UInt32 TransactionID;
        public UInt32 Interval;
        public UInt32 Leechers;
        public UInt32 Seeders;
        public List<TorrentPeer> IPPairs;

        public AnnounceResponse(UInt32 transaction, UInt32 interval, UInt32 leechers, UInt32 seeders, List<TorrentPeer> ips)
        {
            Action = 1;
            TransactionID = transaction;
            Interval = interval;
            Leechers = leechers;
            Seeders = seeders;
            IPPairs = ips;

            byte[] IPBytes = IPPairs.SelectMany(byteArr => byteArr.GetFormattedPair()).ToArray();

            Data = Pack.UInt32(Action).
                Concat(Pack.UInt32(TransactionID)).
                Concat(Pack.UInt32(Interval)).
                Concat(Pack.UInt32(Leechers)).
                Concat(Pack.UInt32(Seeders)).
                Concat(IPBytes).ToArray();
        }

        public AnnounceResponse(UInt32 transaction, UInt32 interval, UInt32 leechers, UInt32 seeders, TorrentPeer ip)
        {
            Action = 1;
            TransactionID = transaction;
            Interval = interval;
            Leechers = leechers;
            Seeders = seeders;
            IPPairs = new List<TorrentPeer>() { ip };

            byte[] IPBytes = IPPairs.SelectMany(byteArr => byteArr.GetFormattedPair()).ToArray();

            Data = Pack.UInt32(Action).
                Concat(Pack.UInt32(TransactionID)).
                Concat(Pack.UInt32(Interval)).
                Concat(Pack.UInt32(Leechers)).
                Concat(Pack.UInt32(Seeders)).
                Concat(IPBytes).ToArray();
        }
    }
}
