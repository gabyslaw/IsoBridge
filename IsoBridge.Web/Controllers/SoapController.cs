using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using IsoBridge.Core.Models.Soap;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IsoBridge.Web.Controllers
{
    // Minimal SOAP 1.1 endpoint: POST /soap/paymentauth
    // Content-Type: text/xml; charset=utf-8
    // SOAPAction: "urn:isobridge:payments/PaymentAuth"
    [ApiController]
    [Route("soap")]
    public class SoapController : ControllerBase
    {
        [HttpPost("paymentauth")]
        public IActionResult PaymentAuth([FromBody] string xmlBody)
        {
            try
            {
                // parse SOAP Body -> PaymentAuthRequest
                var request = DeserializeSoapBody<PaymentAuthRequest>(xmlBody, "PaymentAuthRequest", "urn:isobridge:payments");
                if (request is null)
                    return BadRequest(BuildSoapFault("Client", "Invalid SOAP body"));

                // simple demo logic: approve amounts <= 10000 (i.e. 100.00) - will remove later
                var amount = request.Amount ?? "0";
                var approved = amount.All(char.IsDigit) && amount.Length >= 1 && string.CompareOrdinal(amount, "000000010000") <= 0;

                var response = new PaymentAuthResponse
                {
                    ApprovalCode = approved ? "123456" : "000000",
                    ResponseCode = approved ? "00" : "05",
                    Message = approved ? "Approved" : "Do Not Honor"
                };

                var envelope = WrapInSoapEnvelope(response, "PaymentAuthResponse", "urn:isobridge:payments");
                return Content(envelope, "text/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return Content(BuildSoapFault("Server", ex.Message), "text/xml", Encoding.UTF8);
            }
        }

        private static T? DeserializeSoapBody<T>(string soapXml, string localName, string ns)
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapXml);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
            nsmgr.AddNamespace("p", ns);

            var node = doc.SelectSingleNode($"//s:Body/p:{localName}", nsmgr);
            if (node is null) return default;

            var serializer = new XmlSerializer(typeof(T), ns);
            using var reader = new XmlNodeReader(node);
            return (T?)serializer.Deserialize(reader);
        }

        private static string WrapInSoapEnvelope<T>(T payload, string rootName, string ns)
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootName) { Namespace = ns });
            using var sw = new Utf8StringWriter();
            using var xw = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true });
            sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sw.Write("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            sw.Write("<soap:Body>");
            serializer.Serialize(xw, payload);
            xw.Flush();
            sw.Write("</soap:Body></soap:Envelope>");
            return sw.ToString();
        }

        private static string BuildSoapFault(string code, string message)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
                    <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                    <soap:Body>
                        <soap:Fault>
                        <faultcode>{SecurityElement.Escape(code)}</faultcode>
                        <faultstring>{SecurityElement.Escape(message)}</faultstring>
                        </soap:Fault>
                    </soap:Body>
                    </soap:Envelope>";
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}