using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.ISO8583
{
    public sealed class Iso8583Options
    {
        public string TemplatePath { get; set; } = "Config/iso8583-templates.json";
        public bool UseBcd { get; set; } = false;
    }
}