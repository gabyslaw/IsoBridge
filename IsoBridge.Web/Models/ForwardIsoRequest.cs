using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Web.Models
{
    /// <summary>
    /// Forward an ISO message to an upstream route.
    /// mode = "iso" (payload+encoding) or "json" (mti+fields)
    /// routeKey selects a configured upstream.
    /// </summary>
    public sealed class ForwardIsoRequest
    {
        public string RouteKey { get; set; } = string.Empty;
        public string Mode { get; set; } = "iso"; // "iso" | "json"

        // iso mode
        public string? Payload { get; set; }
        public string Encoding { get; set; } = "hex"; // hex | base64

        // json mode
        public string? Mti { get; set; }
        public Dictionary<int, string>? Fields { get; set; }

        // optional metadata (stored in audit MetaJson)
        public Dictionary<string, string>? Meta { get; set; }
    }
}