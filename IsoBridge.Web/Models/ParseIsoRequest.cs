using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Web.Models
{
    public class ParseIsoRequest
    {
        public string Payload { get; set; } = string.Empty; // hex or base64
        public string Encoding { get; set; } = "hex";
    }
}