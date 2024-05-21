using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using IsoBridge.Core.Models.Soap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IsoBridge.Adapters.Forwarding
{
    public sealed class SoapForwarder
    {
        private readonly IHttpClientFactory _factory;
        private readonly IOptionsMonitor<ForwardingOptions> _opts;
        private readonly ILogger<SoapForwarder>? _logger;

        public SoapForwarder(IHttpClientFactory factory, IOptionsMonitor<ForwardingOptions> opts, ILogger<SoapForwarder>? logger = null)
        {
            _factory = factory;
            _opts = opts;
            _logger = logger;
        }

        public async Task<ForwardResult> ForwardAsync(string routeKey, JsonObject mappedJson, CancellationToken ct = default)
        {
            if (!_opts.CurrentValue.Routes.TryGetValue(routeKey, out var route))
                throw new InvalidOperationException($"Route '{routeKey}' not found.");
            if (!string.Equals(route.Type, "soap", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"Route '{routeKey}' is not SOAP.");

            var client = _factory.CreateClient("IsoBridge.Forwarder");
            if (!Uri.TryCreate(route.BaseAddress, UriKind.Absolute, out var baseUri))
                throw new InvalidOperationException($"Invalid BaseAddress for route '{routeKey}': {route.BaseAddress}");
            client.BaseAddress = baseUri;

            var relative = route.Path.StartsWith("/") ? route.Path : "/" + route.Path;

            // map mappedJson -> PaymentAuthRequest
            var reqDto = BuildPaymentAuthRequestFromMappedJson(mappedJson);

            // wrap into SOAP envelope
            var soapXml = BuildSoapEnvelope(reqDto, "PaymentAuthRequest", "urn:isobridge:payments");

            using var req = new HttpRequestMessage(new HttpMethod(route.Method), relative);
            req.Content = new StringContent(soapXml, Encoding.UTF8, "text/xml");
            if (!string.IsNullOrWhiteSpace(route.SoapAction))
                req.Headers.TryAddWithoutValidation("SOAPAction", route.SoapAction);

            if (route.Headers is not null)
            {
                foreach (var (k, v) in route.Headers)
                    req.Headers.TryAddWithoutValidation(k, v);
            }

            var rsp = await client.SendAsync(req, ct);
            var body = await rsp.Content.ReadAsStringAsync(ct);
            var headers = rsp.Headers.Concat(rsp.Content.Headers)
                .GroupBy(h => h.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(x => x.Value));

            _logger?.LogInformation("SOAP forwarded to {Route} status {Status}", routeKey, (int)rsp.StatusCode);
            return new ForwardResult((int)rsp.StatusCode, body, headers);
        }

        private static PaymentAuthRequest BuildPaymentAuthRequestFromMappedJson(JsonObject root)
        {
            // root: { "mti": "0100", "fields": { "2": "...", "4": "...", "41": "...", "49": "..." } }
            var fields = root["fields"] as JsonObject ?? new JsonObject();
            string Get(string key) => (fields[key]?.GetValue<string>()) ?? string.Empty;

            return new PaymentAuthRequest
            {
                Pan = Get("2"),
                Amount = Get("4"),
                TerminalId = Get("41"),
                Currency = Get("49")
            };
        }

        private static string BuildSoapEnvelope<T>(T payload, string rootName, string ns)
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootName) { Namespace = ns });
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };

            using var sw = new StringWriter();
            using var xw = XmlWriter.Create(sw, settings);
            sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sw.Write("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            sw.Write("<soap:Body>");
            serializer.Serialize(xw, payload);
            xw.Flush();
            sw.Write("</soap:Body></soap:Envelope>");
            return sw.ToString();
        }
    }
}