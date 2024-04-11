using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using IsoBridge.Core.Audit;
using Microsoft.EntityFrameworkCore;

namespace IsoBridge.Infrastructure.Audit
{
    /// <summary>
    /// Append-only repository for writing and reading audit entries.
    /// This prevents updates or deletes and ensures the hash chain stays consistent.
    /// </summary>
    public sealed class AuditRepository : IAuditAppendOnlyStore
    {
        private readonly AuditDbContext _db;
        private readonly IAuditHasher _hasher;

        public AuditRepository(AuditDbContext db, IAuditHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        public async Task AppendAsync(AuditEntry entry, CancellationToken ct = default)
        {
            // find last entry to chain hashes
            var prev = await _db.AuditEntries
                .AsNoTracking()
                .OrderByDescending(e => e.TimestampUtc)
                .FirstOrDefaultAsync(ct);

            var prevHash = prev?.Hash ?? string.Empty;

            entry.PrevHash = prevHash;
            entry.Hash = _hasher.ComputeHash(entry, prevHash);
            entry.HmacSignature = _hasher.ComputeHmac(entry.Hash);
            entry.TimestampUtc = DateTime.UtcNow;

            await _db.AuditEntries.AddAsync(entry, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async IAsyncEnumerable<AuditEntry> QueryAsync(
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var query = _db.AuditEntries.AsNoTracking().AsQueryable();

            if (fromUtc.HasValue)
                query = query.Where(a => a.TimestampUtc >= fromUtc.Value);

            if (toUtc.HasValue)
                query = query.Where(a => a.TimestampUtc <= toUtc.Value);

            await foreach (var item in query.OrderBy(a => a.TimestampUtc).AsAsyncEnumerable().WithCancellation(ct))
                yield return item;
        }
    }
}