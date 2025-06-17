using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs
{
    public class ReceiverAddress
    {
        public String country { get; set; }
        public String governate { get; set; }
        public String regionCity { get; set; }
        public String street { get; set; }
        public String buildingNumber { get; set; }
        public String postalCode { get; set; }
        public String floor { get; set; }
        public String room { get; set; }
        public String landmark { get; set; }
        public String additionalInformation { get; set; }
    }
}