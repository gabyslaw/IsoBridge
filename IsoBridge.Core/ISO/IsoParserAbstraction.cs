using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Core.Models;

namespace IsoBridge.Core.ISO
{
    public interface IIsoParser
    {
        ParseResult Parse(ReadOnlySpan<byte> isoBytes);
        byte[] Build(IsoMessage message);
    }
}