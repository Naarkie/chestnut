using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chestnut.Util;

namespace Chestnut.Packets
{
    public class ConnectRequest : Packet
    {
        public UInt64 ConnectionID;
        public ConnectRequest(byte[] response)
        {
            ConnectionID = Unpack.UInt64(response, 0);
            Action = Unpack.UInt32(response, 8);
            TransactionID = Unpack.UInt32(response, 12);
        }

    }
}
