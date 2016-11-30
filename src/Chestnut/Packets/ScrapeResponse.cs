using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chestnut.Util;
using Chestnut.Torrent;
using System.Net;

namespace Chestnut.Packets
{
    public class ScrapeResponse : Packet
    {
        public byte[] Data;
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
