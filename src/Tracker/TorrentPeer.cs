using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Util;
using System.Net;

namespace Tracker
{

    public class TorrentPeer
    {
        public byte[] IP;
        public byte[] Port;
        public TorrentPeer(int ip, short port)
        {
            IP = Pack.Int32(ip);
            Port = Pack.Int16(port);
        }

        public TorrentPeer(string ip, short port)
        {
            IP = IPAddress.Parse(ip).GetAddressBytes();
            Port = Pack.Int16(port);
        }

        public TorrentPeer(IPAddress ip, short port)
        {
            IP = ip.GetAddressBytes();
            Port = Pack.Int16(port);
        }

        public TorrentPeer(string redisResponse)
        {
            var parts = redisResponse.Split(':');
            IP = IPAddress.Parse(parts[0]).GetAddressBytes();
            Port = Pack.Int16(short.Parse(parts[1]));
        }

        public TorrentPeer(byte[] peer)
        {
            IP = UtilityFunctions.GetBytes(peer, 0, 4);
            Port = UtilityFunctions.GetBytes(peer, 4, 2);
        }

        public string StringPeer()
        {
            short port = Unpack.Int16(Port, 0);
            return GetIPString() + ":" + port.ToString();
        }

        public byte[] GetFormattedPair()
        {
            return UtilityFunctions.Cat(IP, Port);
        }

        public string GetIPString()
        {
            return new IPAddress(IP).ToString();
        }
    }
}
