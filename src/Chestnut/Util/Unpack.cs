﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chestnut.Util
{
    public static class Unpack
    {
        public enum Endianness
        {
            Machine,
            Big,
            Little
        }

        public static Int16 Int16(byte[] bytes, Int32 start, Endianness e = Endianness.Big)
        {
            byte[] intBytes = UtilityFunctions.GetBytes(bytes, start, 2);

            if (NeedsFlipping(e)) Array.Reverse(intBytes);

            return BitConverter.ToInt16(intBytes, 0);
        }

        public static Int32 Int32(byte[] bytes, Int32 start, Endianness e = Endianness.Big)
        {
            byte[] intBytes = UtilityFunctions.GetBytes(bytes, start, 4);

            if (NeedsFlipping(e)) Array.Reverse(intBytes);

            return BitConverter.ToInt32(intBytes, 0);
        }

        public static Int64 Int64(byte[] bytes, Int32 start, Endianness e = Endianness.Big)
        {
            byte[] intBytes = UtilityFunctions.GetBytes(bytes, start, 8);

            if (NeedsFlipping(e)) Array.Reverse(intBytes);

            return BitConverter.ToInt64(intBytes, 0);
        }

        public static UInt16 UInt16(byte[] bytes, Int32 start, Endianness e = Endianness.Big)
        {
            byte[] intBytes = UtilityFunctions.GetBytes(bytes, start, 2);

            if (NeedsFlipping(e)) Array.Reverse(intBytes);

            return BitConverter.ToUInt16(intBytes, 0);
        }

        public static UInt32 UInt32(byte[] bytes, Int32 start, Endianness e = Endianness.Big)
        {
            byte[] intBytes = UtilityFunctions.GetBytes(bytes, start, 4);

            if (NeedsFlipping(e)) Array.Reverse(intBytes);

            return BitConverter.ToUInt32(intBytes, 0);
        }

        public static UInt64 UInt64(byte[] bytes, Int32 start, Endianness e = Endianness.Big)
        {
            byte[] intBytes = UtilityFunctions.GetBytes(bytes, start, 8);

            if (NeedsFlipping(e)) Array.Reverse(intBytes);

            return BitConverter.ToUInt64(intBytes, 0);
        }

        private static bool NeedsFlipping(Endianness e)
        {
            switch (e)
            {
                case Endianness.Big:
                    return BitConverter.IsLittleEndian;
                case Endianness.Little:
                    return !BitConverter.IsLittleEndian;
            }

            return false;
        }
        public static String Hex(byte[] bytes, Endianness e = Endianness.Big)
        {
            String str = "";

            foreach (byte b in bytes)
            {
                str += String.Format("{0:X2}", b);
            }

            return str;
        }
    }
 }