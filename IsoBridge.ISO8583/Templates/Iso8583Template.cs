using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.ISO8583.Templates
{
    public sealed class FieldTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "ans";
        public int Length { get; set; }
        public bool Variable { get; set; }
        public int VarLengthDigits { get; set; }
    }

    public sealed class Iso8583Template
    {
        public string Mti { get; set; } = string.Empty;
        public List<FieldTemplate> Fields { get; set; } = new();
    }
}