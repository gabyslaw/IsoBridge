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

        public async Task<(bool IsValid, string Message)> VerifyAsync()
        {
            var entries = await _db.AuditEntries
                .OrderBy(e => e.TimestampUtc)
                .ToListAsync();

            if (entries.Count == 0)
                return (true, "No audit entries to verify.");

            string? prevHash = null;
            foreach (var entry in entries)
            {
                // recompute hash for this entry
                var computedHash = _hasher.ComputeHash(entry, prevHash ?? string.Empty);

                // verify linkage
                if (prevHash != null && entry.PrevHash != prevHash)
                {
                    _logger?.LogWarning("Broken chain detected at {Id}", entry.Id);
                    return (false, $"Chain tamper detected at entry {entry.Id}");
                }

                // verify hash integrity
                if (!string.Equals(entry.Hash, computedHash, StringComparison.Ordinal))
                {
                    _logger?.LogWarning("Hash mismatch at {Id}", entry.Id);
                    return (false, $"Tamper detected at entry {entry.Id}");
                }

                // verify HMAC signature
                var expectedHmac = _hasher.ComputeHmac(entry.Hash);
                if (!string.Equals(entry.HmacSignature, expectedHmac, StringComparison.Ordinal))
                {
                    _logger?.LogWarning("HMAC mismatch at {Id}", entry.Id);
                    return (false, $"HMAC mismatch at entry {entry.Id}");
                }

                prevHash = entry.Hash;
            }

            return (true, "Chain verified successfully.");
        }
    }

    public sealed record AuditVerificationResult(bool IsValid, int EntriesChecked, string Message);

}