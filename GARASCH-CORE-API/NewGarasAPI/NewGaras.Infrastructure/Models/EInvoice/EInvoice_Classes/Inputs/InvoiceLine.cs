using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs
{
    public class InvoiceLine
    {
        public string description { get; set; }
        public string itemType { get; set; }
        public string itemCode { get; set; }
        public string unitType { get; set; }
        public decimal quantity { get; set; }
        public string internalCode { get; set; }
        public decimal salesTotal { get; set; }
        public decimal total { get; set; }
        public decimal valueDifference { get; set; }
        public decimal totalTaxableFees { get; set; }
        public decimal netTotal { get; set; }
        public decimal itemsDiscount { get; set; }
        public UnitValue unitValue { get; set; }
        public Discount discount { get; set; }
        public List<TaxableItem> taxableItems { get; set; }

    }
}