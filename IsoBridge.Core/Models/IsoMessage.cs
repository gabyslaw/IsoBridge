using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Core.Models
{
    public sealed record IsoMessage(
        string Mti,
        IReadOnlyDictionary<int, string> Fields
    );
}