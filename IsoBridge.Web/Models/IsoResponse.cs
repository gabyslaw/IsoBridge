using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Web.Models
{
    public class IsoResponse
    {
        public string Mti { get; set; } = string.Empty;
        public Dictionary<int, string>? Fields { get; set; }
        public string? Hex { get; set; }
        public string? Base64 { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}