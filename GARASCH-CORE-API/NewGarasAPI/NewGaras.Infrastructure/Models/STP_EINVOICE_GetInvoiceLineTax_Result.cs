using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class STP_EINVOICE_GetInvoiceLineTax_Result
    {
        public string SubTaxName { get; set; }
        public Nullable<decimal> TaxPercentage { get; set; }
        public string TaxType { get; set; }
        public Nullable<decimal> TaxValue { get; set; }
        public string TaxName { get; set; }
        public Nullable<bool> isPercentage { get; set; }
        public int LastOne { get; set; }
    }
}
