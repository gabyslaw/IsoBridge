using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Core.Audit
{
    public sealed class AuditEntry
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string Actor { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string RequestDigest { get; set; } = string.Empty;
        public string ResponseDigest { get; set; } = string.Empty;
        public string PrevHash { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string HmacSignature { get; set; } = string.Empty;
        public string MetaJson { get; set; } = "{}";
    }
}