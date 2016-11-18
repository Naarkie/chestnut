using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;
using Tracker.Torrent;
using System.Net;

namespace Tracker.Packets
{
    public class ScrapeResponse
    {
        public byte[] Data;

        public UInt32 TransactionID;
        public UInt32 Action;
        public List<TorrentInfo> ScrapeInfo;

        public ScrapeResponse(UInt32 transactionID, List<TorrentInfo> scrapeInfo)
        {
            Action = 2; //scrape
            TransactionID = transactionID;

            byte[] ScrapeBytes = scrapeInfo.SelectMany(torrent => torrent.PackedTorrentInfo()).ToArray();

            Data = Pack.UInt32(Action).
                Concat(Pack.UInt32(Action)).
                Concat(Pack.UInt32(transactionID)).
                Concat(ScrapeBytes).ToArray();
        }
    }
}
