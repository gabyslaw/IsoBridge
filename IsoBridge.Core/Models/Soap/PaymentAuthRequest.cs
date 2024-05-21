using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IsoBridge.Core.Models.Soap
{
    [XmlRoot("PaymentAuthRequest", Namespace = "urn:isobridge:payments")]
    public class PaymentAuthRequest
    {
        [XmlElement("Pan")]
        public string Pan { get; set; } = string.Empty;

        [XmlElement("Amount")]
        public string Amount { get; set; } = string.Empty; // e.g. "000000010000"

        [XmlElement("Currency")]
        public string Currency { get; set; } = string.Empty; // e.g. "840"

        [XmlElement("TerminalId")]
        public string TerminalId { get; set; } = string.Empty; // e.g. "TERM001"
    }
}