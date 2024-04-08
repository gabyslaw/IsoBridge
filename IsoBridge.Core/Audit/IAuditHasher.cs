using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Core.Audit
{
    public interface IAuditHasher
    {
        string ComputeHash(AuditEntry entry, string prevHash);
        string ComputeHmac(string data);
    }
}