using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;

namespace Tracker.Packets
{
    public class ScrapeRequest
    {
        public UInt64 ConnectionID;
        public UInt32 Action;
        public UInt32 TransactionID;
        public List<byte[]> InfoHashes = new List<byte[]>();


        public ScrapeRequest(byte[] data)
        {
            ConnectionID = Unpack.UInt64(data, 0);
            Action = Unpack.UInt32(data, 8);
            TransactionID = Unpack.UInt32(data, 12);
            
            for(int i = 16; i < (data.Length - 16 - (20 * i)); i += 20)
            {
                byte[] hash = UtilityFunctions.GetBytes(data, i, 20);
                InfoHashes.Add(hash);
            }
        }
    }
}
