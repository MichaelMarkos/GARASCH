using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs
{
    public class TaxableItem
    {
        public String taxType { get; set; }
        public Decimal amount { get; set; }
        public String subType { get; set; }
        public Decimal rate { get; set; }
    }
}