using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                RequestDigest = SensitiveDataMasker.Mask(request),
                ResponseDigest = SensitiveDataMasker.Mask(response),
                MetaJson = meta
            };
            await _store.AppendAsync(entry);
        }

        internal static class SensitiveDataMasker
        {
            // Simple masking for PAN/track2/PIN-like hex in audit digests.
            private static readonly Regex PanPattern = new(@"(?<!\d)(\d{6})\d{6,9}(\d{4})(?!\d)", RegexOptions.Compiled);
            private static readonly Regex Track2Pattern = new(@"(;?\d{6})\d{6,9}(=)", RegexOptions.Compiled);
            private static readonly Regex LongHexPattern = new(@"\b([A-Fa-f0-9]{4})[A-Fa-f0-9]{8,}([A-Fa-f0-9]{4})\b", RegexOptions.Compiled);

            public static string Mask(string input)
            {
                if (string.IsNullOrEmpty(input)) return input ?? string.Empty;
                var masked = PanPattern.Replace(input, "$1******$2");
                masked = Track2Pattern.Replace(masked, "$1******$2");
                masked = LongHexPattern.Replace(masked, "$1********$2");
                return masked;
            }
        }
    }
}