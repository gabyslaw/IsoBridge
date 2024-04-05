using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private static readonly Regex DigitsOnly = new("^[0-9]+$", RegexOptions.Compiled);

        public byte[] Encode(string value, int length, bool variable, int varDigits, bool bcd)
        {
            string data;

            if (variable)
            {
                data = string.Format("{0:D" + varDigits + "}{1}", value.Length, value);
            }
            else
            {
                // pad numeric with zeros, alphanumeric with spaces
                var padChar = DigitsOnly.IsMatch(value) ? '0' : ' ';
                data = value.PadRight(length, padChar).Substring(0, length);
            }

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

            var fixedVal = Encoding.ASCII.GetString(input[..length]).TrimEnd(' ');
            return (fixedVal, length);
        }
    }
}