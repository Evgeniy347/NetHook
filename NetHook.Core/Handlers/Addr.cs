using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NetHook.Core
{
    public static class Addr
    {
        public static string Make(this byte[] buffer)
        {
            string sTemp = "";

            for (int i = 0; i < buffer.Length; i++)
            {
                if (Convert.ToInt16(buffer[i]) < 10)
                    sTemp = "0" + ToHex(buffer[i]) + sTemp;
                else
                    sTemp = ToHex(buffer[i]) + sTemp;
            }

            return sTemp;
        }

        public static string ToHex(this int Decimal)
        {
            return "0x" + Decimal.ToString("x");
        }
        public static string ToHex(this long Decimal)
        {
            return "0x" + Decimal.ToString("x");
        }

        public static string ToHex(this IntPtr Decimal)
        {
            return "0x" + Decimal.ToString("x");
        }

        public static string ToHexArray(this IEnumerable<byte> bytes)
        {
            if (bytes == null)
                return string.Empty;

            return string.Join(" ", bytes.Select(x => x.ToString("x").PadLeft(2, '0')).ToArray());
        }

        public static int ToDec(this string Hex)
        {
            return int.Parse(Hex, NumberStyles.HexNumber);
        }

        public static long ToDec64(this string Hex)
        {
            return long.Parse(Hex, NumberStyles.HexNumber);
        }

        public static int ToDec(this IEnumerable<byte> bytes)
        {
            List<byte> collection = bytes.ToList();

            for (int i = collection.Count; i < 4; i++)
                collection.Insert(0, 0);

            byte[] array = collection.ToArray();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(array);

            var result = BitConverter.ToInt32(array, 0);
            return result;
        }

        public static long ToDec64(this IEnumerable<byte> bytes)
        {
            byte[] array = bytes.ToArray();

            List<byte> collection = array.ToList();

            for (int i = collection.Count; i < 8; i++)
                collection.Add(0);


            var result = BitConverter.ToInt64(collection.ToArray(), 0);
            return result;
        }

        public static IntPtr ToIntPtr(this string Hex)
        {
            return new IntPtr(ToDec64(Hex));
        }
    }
}
