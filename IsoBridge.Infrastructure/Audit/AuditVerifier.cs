using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Core.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IsoBridge.Infrastructure.Audit
{
    public sealed class AuditVerifier
    {
        private readonly AuditDbContext _db;
        private readonly IAuditHasher _hasher;
        private readonly ILogger<AuditVerifier>? _logger;

        public AuditVerifier(AuditDbContext db, IAuditHasher hasher, ILogger<AuditVerifier>? logger = null)
        {
            _db = db;
            _hasher = hasher;
            _logger = logger;
        }

        public async Task<AuditVerificationResult> VerifyAsync(CancellationToken ct = default)
        {
            var entries = await _db.AuditEntries.AsNoTracking()
                .OrderBy(a => a.TimestampUtc)
                .ToListAsync(ct);

            if (entries.Count == 0)
                return new AuditVerificationResult(true, 0, "No entries found.");

            string prevHash = string.Empty;
            int index = 0;

            foreach (var e in entries)
            {
                index++;
                var expectedHash = _hasher.ComputeHash(e, prevHash);
                if (!string.Equals(expectedHash, e.Hash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogWarning("Hash mismatch at index {Index}", index);
                    return new AuditVerificationResult(false, index, $"Hash mismatch at {e.Id}");
                }

                var expectedHmac = _hasher.ComputeHmac(e.Hash);
                if (!string.Equals(expectedHmac, e.HmacSignature, StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogWarning("HMAC mismatch at index {Index}", index);
                    return new AuditVerificationResult(false, index, $"HMAC mismatch at {e.Id}");
                }

                prevHash = e.Hash;
            }

            return new AuditVerificationResult(true, entries.Count, "Chain verified successfully.");
        }
    }

    public sealed record AuditVerificationResult(bool IsValid, int EntriesChecked, string Message);

}