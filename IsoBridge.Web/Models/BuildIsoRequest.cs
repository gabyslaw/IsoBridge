using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Web.Models
{
    public class BuildIsoRequest
    {
        public string Mti { get; set; } = string.Empty;
        public Dictionary<int, string> Fields { get; set; } = new();
    }
}