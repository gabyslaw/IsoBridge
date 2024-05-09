using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsoBridge.Core.Audit;

namespace IsoBridge.Web.Services
{
    public class AuditLoggingService
    {
        private readonly IAuditAppendOnlyStore _store;
        private readonly IAuditHasher _hasher;

        public AuditLoggingService(IAuditAppendOnlyStore store, IAuditHasher hasher)
        {
            _store = store;
            _hasher = hasher;
        }

        public async Task LogAsync(string actor, string service, string request, string response, string meta)
        {
            var entry = new AuditEntry
            {
                Actor = actor,
                Service = service,
                RequestDigest = request,
                ResponseDigest = response,
                MetaJson = meta
            };
            await _store.AppendAsync(entry);
        }
    }
}