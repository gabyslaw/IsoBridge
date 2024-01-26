using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Core.Audit
{
    public sealed record AuditEntry(
        Guid Id,
        DateTime TimestampUtc,
        string Actor,
        string Service,
        string RequestDigest,
        string ResponseDigest,
        string PrevHash,
        string Hash,
        string HmacSignature,
        string MetaJson
    );

    public interface IAuditAppendOnlyStore
    {
        Task AppendAsync(AuditEntry entry, CancellationToken ct = default);
        IAsyncEnumerable<AuditEntry> QueryAsync(DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default);
    }

    public interface IAuditHasher
    {
        string ComputeHash(string prevHash, DateTime timestampUtc, string requestDigest, string responseDigest, string metaJson);
        string ComputeHmac(string hash);
    }
}