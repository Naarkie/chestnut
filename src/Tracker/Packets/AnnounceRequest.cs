using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;

namespace Tracker.Packets
{
    public class AnnounceRequest
    {
        public Int64 ConnectionID;
        public Int32 Action;
        public Int32 TransactionID;
        public byte[] InfoHash = new byte[20];
        public byte[] PeerID = new byte[20];
        public Int64 Downloaded;
        public Int64 Left;
        public Int64 Uploaded;
        public Int32 TorrentEvent;
        public Int32 IpAddress;
        public Int64 Key;
        public Int32 NumWanted;
        public Int16 Port;


        public AnnounceRequest(byte[] data)
        {
            ConnectionID = Unpack.Int64(data, 0);
            Action = Unpack.Int32(data, 8);
            TransactionID = Unpack.Int32(data, 12);
            InfoHash = UtilityFunctions.GetBytes(data, 16, 20);
            PeerID = UtilityFunctions.GetBytes(data, 36, 20);
            Downloaded = Unpack.Int64(data, 56);
            Left = Unpack.Int64(data, 64);
            Uploaded = Unpack.Int64(data, 72);
            TorrentEvent = Unpack.Int32(data, 80);
            IpAddress = Unpack.Int32(data, 84);
            Key = Unpack.Int64(data, 88);
            NumWanted = Unpack.Int32(data, 92);
            Port = Unpack.Int16(data, 96);
        }
    }
}
