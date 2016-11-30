using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chestnut.Util;
using System.Net;

namespace Chestnut.Torrent
{

    public class TorrentPeer
    {
        public byte[] IP;
        public ushort Port;
        public TorrentPeer(uint ip, ushort port)
        {
            IP = Pack.UInt32(ip);
            Port = port;
        }

        public TorrentPeer(string ip, ushort port)
        {
            IP = IPAddress.Parse(ip).GetAddressBytes();
            Port = port;
        }

        public TorrentPeer(IPAddress ip, ushort port)
        {
            IP = ip.GetAddressBytes();
            Port = port;
        }

        public TorrentPeer(string redisResponse)
        {
            var parts = redisResponse.Split(':');
            IP = IPAddress.Parse(parts[0]).GetAddressBytes();
            Port = (ushort.Parse(parts[1]));
        }

        public TorrentPeer(byte[] peer)
        {
            IP = UtilityFunctions.GetBytes(peer, 0, 4);
            Port = Unpack.UInt16(peer,4);
        }

        public string StringPeer()
        {
            return GetIPString() + ":" + Port.ToString();
        }

        public byte[] GetFormattedPair()
        {
            return UtilityFunctions.Cat(IP, Pack.UInt16(Port));
        }

        public string GetIPString()
        {
            return new IPAddress(IP).ToString();
        }
    }
}
