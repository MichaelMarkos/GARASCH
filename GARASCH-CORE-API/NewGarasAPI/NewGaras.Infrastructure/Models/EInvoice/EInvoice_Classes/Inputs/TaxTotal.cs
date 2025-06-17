using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs
{
    public class TaxTotal
    {
        public String taxType { get; set; }
        public Decimal amount { get; set; }
    }
}