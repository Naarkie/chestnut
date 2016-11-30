using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chestnut.Util;

namespace Chestnut.Packets
{
    public class ConnectResponse : Packet
    {
        public byte[] Data;

        public UInt64 ConnectionID;
        public ConnectResponse(UInt32 action, UInt32 transaction, UInt64 connection)
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
