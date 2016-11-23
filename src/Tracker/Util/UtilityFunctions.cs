using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tracker.Util
{
    public enum TorrentEvent
    {
        None = 0,
        Completed = 1,
        Started = 2,
        Stopped = 3
    }

    public enum PeerType
    {
        Seeder,
        Leecher,
        Unknown
    }
    public static class UtilityFunctions
    {
        public static bool GetBit(this byte t, UInt16 n)
        {
            return (t & (1 << n)) != 0;
        }

        public static byte SetBit(this byte t, UInt16 n)
        {
            return (byte)(t | (1 << n));
        }

        public static byte[] GetBytes(this byte[] bytes, Int32 start, Int32 length = -1)
        {
            int l = length;
            if (l == -1) l = bytes.Length - start;

            byte[] intBytes = new byte[l];

            for (int i = 0; i < l; i++) intBytes[i] = bytes[start + i];

            return intBytes;
        }

        public static byte[] Cat(this byte[] first, byte[] second)
        {
            byte[] returnBytes = new byte[first.Length + second.Length];

            first.CopyTo(returnBytes, 0);
            second.CopyTo(returnBytes, first.Length);

            return returnBytes;
        }
    }
}
