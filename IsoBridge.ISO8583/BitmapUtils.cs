using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.ISO8583
{
    public static class BitmapUtils
    {
        public static byte[] BuildBitmap(IEnumerable<int> fields)
        {
            var max = fields.Any() ? fields.Max() : 0;
            var bitmapLength = max > 64 ? 16 : 8;
            var bitmap = new byte[bitmapLength];
            foreach (var f in fields)
            {
                if (f == 1) continue; // bitmap itself here
                var index = (f - 1) / 8;
                var bit = 7 - ((f - 1) % 8);
                bitmap[index] |= (byte)(1 << bit);
            }
            if (bitmapLength == 16)
                bitmap[0] |= 0x80; // secondary bitmap indicator
            return bitmap;
        }

        public static HashSet<int> ParseBitmap(ReadOnlySpan<byte> bytes)
        {
            var fields = new HashSet<int>();
            var hasSecondary = (bytes[0] & 0x80) != 0;
            var len = hasSecondary ? 16 : 8;
            for (int i = 0; i < len; i++)
            {
                for (int b = 0; b < 8; b++)
                {
                    if ((bytes[i] & (1 << (7 - b))) != 0)
                        fields.Add(i * 8 + b + 1);
                }
            }
            if (hasSecondary) fields.Remove(1);
            return fields;
        }
    }
}