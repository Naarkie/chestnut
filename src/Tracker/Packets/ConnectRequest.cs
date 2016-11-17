using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;

namespace Tracker.Packets
{
    public class ConnectRequest
    {
        public Int64 ConnectionID;
        public Int32 Action;
        public Int32 TransactionID;
        public ConnectRequest(byte[] response)
        {
            ConnectionID = Unpack.Int64(response, 0);
            Action = Unpack.Int32(response, 8);
            TransactionID = Unpack.Int32(response, 12);
        }

    }
}
