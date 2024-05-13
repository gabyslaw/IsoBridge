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
        public string Type { get; set; } = "rest";
        public string BaseAddress { get; set; } = string.Empty; 
        public string Path { get; set; } = "/";                 // e.g. /post
        public string Method { get; set; } = "POST";
        public Dictionary<string, string>? Headers { get; set; }

        /// <summary>If true, send raw ISO bytes (application/octet-stream); else send mapped JSON.</summary>
        public bool SendRaw { get; set; } = false;
    }
}