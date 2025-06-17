using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ItemsPricing.UsedInResponse
{
    public class PurchasePOInventoryItemPrice
    {
        public long? SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceDate {  get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public long? PurchasePOInvoiceID { get; set; }
    }
}
