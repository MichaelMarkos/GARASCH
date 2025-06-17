using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs
{
    public class Issuer
    {
        public IssuerAddress address { get; set; }
        public String type { get; set; }
        public String id { get; set; }
        public String name { get; set; }
    }
}