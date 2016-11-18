using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;
using Tracker.Torrent;

namespace Tracker.Packets
{
    public class AnnounceRequest
    {
        public UInt64 ConnectionID;
        public UInt32 Action;
        public UInt32 TransactionID;
        public byte[] InfoHash = new byte[20];
        public byte[] PeerID = new byte[20];
        public UInt64 Downloaded;
        public UInt64 Left;
        public UInt64 Uploaded;
        public UInt32 TorrentEvent;
        public UInt32 IpAddress;
        public UInt64 Key;
        public UInt32 NumWanted;
        public UInt16 Port;


        public AnnounceRequest(byte[] data)
        {
            ConnectionID = Unpack.UInt64(data, 0);
            Action = Unpack.UInt32(data, 8);
            TransactionID = Unpack.UInt32(data, 12);
            InfoHash = UtilityFunctions.GetBytes(data, 16, 20);
            PeerID = UtilityFunctions.GetBytes(data, 36, 20);
            Downloaded = Unpack.UInt64(data, 56);
            Left = Unpack.UInt64(data, 64);
            Uploaded = Unpack.UInt64(data, 72);
            TorrentEvent = Unpack.UInt32(data, 80);
            IpAddress = Unpack.UInt32(data, 84);
            Key = Unpack.UInt64(data, 88);
            NumWanted = Unpack.UInt32(data, 92);
            Port = Unpack.UInt16(data, 96);

            if(Port < 0)
            {
            //    Port += ushort.MaxValue;
            }
        }
    }
}
