using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoBridge.ISO8583
{
    public interface IFieldCodec
    {
        byte[] Encode(string value, int length, bool variable, int varDigits, bool bcd);
        (string value, int bytesUsed) Decode(ReadOnlySpan<byte> input, int length, bool variable, int varDigits, bool bcd);
    }

    public sealed class DefaultFieldCodec : IFieldCodec
    {
        public byte[] Encode(string value, int length, bool variable, int varDigits, bool bcd)
        {
            var data = variable
                ? string.Format("{0:D" + varDigits + "}{1}", value.Length, value)
                : value.PadLeft(length, '0');
            return Encoding.ASCII.GetBytes(data);
        }

        public (string value, int bytesUsed) Decode(ReadOnlySpan<byte> input, int length, bool variable, int varDigits, bool bcd)
        {
            if (variable)
            {
                var lenPrefix = Encoding.ASCII.GetString(input[..varDigits]);
                var fieldLen = int.Parse(lenPrefix);
                var val = Encoding.ASCII.GetString(input.Slice(varDigits, fieldLen));
                return (val, varDigits + fieldLen);
            }
            var fixedVal = Encoding.ASCII.GetString(input[..length]);
            return (fixedVal, length);
        }
    }
}