using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;

namespace Tracker.Packets
{
    public class ConnectRequest
    {
        public UInt64 ConnectionID;
        public UInt32 Action;
        public UInt32 TransactionID;
        public ConnectRequest(byte[] response)
        {
            ConnectionID = Unpack.UInt64(response, 0);
            Action = Unpack.UInt32(response, 8);
            TransactionID = Unpack.UInt32(response, 12);
        }

    }
}
