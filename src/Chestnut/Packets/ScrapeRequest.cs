using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chestnut.Util;

namespace Chestnut.Packets
{
    public class ScrapeRequest : Packet
    {
        public UInt64 ConnectionID;
        public List<byte[]> InfoHashes = new List<byte[]>();


        public ScrapeRequest(byte[] data)
        {
            ConnectionID = Unpack.UInt64(data, 0);
            Action = Unpack.UInt32(data, 8);
            TransactionID = Unpack.UInt32(data, 12);

            int totalHashes = (data.Length - 16) / 20;
            for(int i = 0; i < totalHashes; i += 1)
            {
                byte[] hash = UtilityFunctions.GetBytes(data, 16 + (i * 20), 20);
                InfoHashes.Add(hash);
            }
        }
    }
}
