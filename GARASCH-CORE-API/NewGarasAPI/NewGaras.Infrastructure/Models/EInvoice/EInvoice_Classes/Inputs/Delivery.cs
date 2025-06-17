using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs
{
    public class Delivery
    {
        public string approach { get; set; }
        public string packaging { get; set; }
        public string dateValidity { get; set; }
        public string exportPort { get; set; }
        //    public String countryOfOrigin { get; set; }
        public decimal grossWeight { get; set; }
        public decimal netWeight { get; set; }
        public string terms { get; set; }
    }
}