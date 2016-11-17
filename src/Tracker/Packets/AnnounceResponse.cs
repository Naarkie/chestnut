using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;
using System.Net;

namespace Tracker.Packets
{
    public class AnnounceResponse
    {
        public byte[] Data;

        public Int32 Action;
        public Int32 TransactionID;
        public Int32 Interval;
        public Int32 Leechers;
        public Int32 Seeders;
        public List<TorrentPeer> IPPairs;

        public AnnounceResponse(Int32 transaction, Int32 interval, Int32 leechers, Int32 seeders, List<TorrentPeer> ips)
        {
            Action = 1;
            TransactionID = transaction;
            Interval = interval;
            Leechers = leechers;
            Seeders = seeders;
            IPPairs = ips;

            byte[] IPBytes = IPPairs.SelectMany(byteArr => byteArr.GetFormattedPair()).ToArray();

            Data = Pack.Int32(Action).
                Concat(Pack.Int32(TransactionID)).
                Concat(Pack.Int32(Interval)).
                Concat(Pack.Int32(Leechers)).
                Concat(Pack.Int32(Seeders)).
                Concat(IPBytes).ToArray();
        }

        public AnnounceResponse(Int32 transaction, Int32 interval, Int32 leechers, Int32 seeders, TorrentPeer ip)
        {
            Action = 1;
            TransactionID = transaction;
            Interval = interval;
            Leechers = leechers;
            Seeders = seeders;
            IPPairs = new List<TorrentPeer>() { ip };

            byte[] IPBytes = IPPairs.SelectMany(byteArr => byteArr.GetFormattedPair()).ToArray();

            Data = Pack.Int32(Action).
                Concat(Pack.Int32(TransactionID)).
                Concat(Pack.Int32(Interval)).
                Concat(Pack.Int32(Leechers)).
                Concat(Pack.Int32(Seeders)).
                Concat(IPBytes).ToArray();
        }
    }
}
