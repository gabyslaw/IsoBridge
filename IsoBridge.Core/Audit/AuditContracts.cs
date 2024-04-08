using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Core.Audit
{   

    public interface IAuditAppendOnlyStore
    {
        Task AppendAsync(AuditEntry entry, CancellationToken ct = default);
        IAsyncEnumerable<AuditEntry> QueryAsync(DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default);
    }

}