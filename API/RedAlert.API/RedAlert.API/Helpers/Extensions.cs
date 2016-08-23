using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RedAlert.API.Helpers
{
    public static class Extensions
    {
        public static string ByteToHex(this byte[] array)
        {
            string output = string.Empty;

            foreach (byte b in array)
            {
                output += b.ToString("x2");
            }

            return output;
        }
        public static byte[] StringToByteArray(this string hex)
        {
            if (hex.Length % 2 != 0) throw new FormatException("Not a hex string.");

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}