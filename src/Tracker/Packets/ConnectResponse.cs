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

        public Int32 Action;
        public Int32 TransactionID;
        public Int64 ConnectionID;
        public ConnectResponse(Int32 action, Int32 transaction, Int64 connection)
        {
            Action = action;
            TransactionID = transaction;
            ConnectionID = connection;

            Data = Pack.Int32(Action).
                Concat(Pack.Int32(TransactionID)).
                Concat(Pack.Int64(ConnectionID)).ToArray();
        }
    }
}
