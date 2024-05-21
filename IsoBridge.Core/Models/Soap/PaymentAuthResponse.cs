using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IsoBridge.Core.Models.Soap
{
    [XmlRoot("PaymentAuthResponse", Namespace = "urn:isobridge:payments")]
    public class PaymentAuthResponse
    {
        [XmlElement("ApprovalCode")]
        public string ApprovalCode { get; set; } = string.Empty; // maps to DE38

        [XmlElement("ResponseCode")]
        public string ResponseCode { get; set; } = string.Empty; // maps to DE39 (00, 05, etc.)

        [XmlElement("Message")]
        public string Message { get; set; } = string.Empty; // human-friendly
    }
}