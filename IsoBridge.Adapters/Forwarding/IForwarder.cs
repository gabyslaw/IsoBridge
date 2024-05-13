using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace IsoBridge.Adapters.Forwarding
{
    public interface IForwarder
    {
        Task<ForwardResult> ForwardAsync(string routeKey, byte[] isoBytes, JsonObject mappedJson, CancellationToken ct = default);
    }

    public sealed record ForwardResult(int StatusCode, string Body, Dictionary<string, IEnumerable<string>> Headers);

}