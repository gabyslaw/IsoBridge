using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Core.Models
{
    public sealed record ParseResult(
        bool Success,
        IsoMessage? Message,
        string? Error
    );
}