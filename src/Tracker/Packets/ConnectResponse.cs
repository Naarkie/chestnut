using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;

namespace Tracker.Packets
{
    public class ConnectResponse
    {
        public byte[] Data;

        public UInt32 Action;
        public UInt32 TransactionID;
        public UInt64 ConnectionID;
        public ConnectResponse(UInt32 action, UInt32 transaction, Int64 connection)
        {
            Action = action;
            TransactionID = transaction;
            ConnectionID = connection;

            Data = Pack.UInt32(Action).
                Concat(Pack.UInt32(TransactionID)).
                Concat(Pack.UInt64(ConnectionID)).ToArray();
        }
    }
}
