using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsoBridge.Adapters.Forwarding
{
    public sealed class ForwardingOptions
    {
        public Dictionary<string, Route> Routes { get; set; } = new();
    }

    public sealed class Route
    {
        /// <summary>"rest" or "soap"</summary>
        public string Type { get; set; } = "rest";

        // common
        public string BaseAddress { get; set; } = string.Empty;
        public string Path { get; set; } = "/";
        public string Method { get; set; } = "POST";
        public Dictionary<string, string>? Headers { get; set; }

        // rest
        public bool SendRaw { get; set; } = false;

        // soap
        public string? SoapAction { get; set; } // e.g. "urn:isobridge:payments/PaymentAuth"
    }
}