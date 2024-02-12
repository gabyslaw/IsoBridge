using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Core.ISO;
using IsoBridge.Core.Models;
using Microsoft.Extensions.Options;

namespace IsoBridge.ISO8583
{
    public sealed class DefaultIsoParser : IIsoParser
    {
        private readonly Iso8583Options _opts;

        public DefaultIsoParser(IOptions<Iso8583Options> options) => _opts = options.Value;

        public ParseResult Parse(ReadOnlySpan<byte> isoBytes)
        {
            // Stubbing this for now
            return new ParseResult(true, new IsoMessage("0000", new Dictionary<int, string>()), null);
        }

        public byte[] Build(IsoMessage message)
        {
            // Stubbing this for now
            return Array.Empty<byte>();
        }
    }
}