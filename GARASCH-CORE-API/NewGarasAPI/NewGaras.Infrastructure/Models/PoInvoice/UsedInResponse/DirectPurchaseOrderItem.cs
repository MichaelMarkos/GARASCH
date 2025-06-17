using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse
{
    public class DirectPurchaseOrderItem
    {
        public long InventoryItemID;

        public decimal? EstimatedUnitCost;
        public int? CurrencyID;


        public decimal? RecivedQuantity;
        public decimal? ReqQuantity;

        public bool? IsChecked;
        public string SupplierInvoiceSerial;
        public decimal? RateToEgp { get; set; }
        public decimal? ActualLocalUnitPrice { get; set; }
        public decimal? ActualUnitPrice { get; set; }
        public decimal? TotalLocalActualPrice { get; set; }
        public decimal? FinalLocalUnitCostUOR { get; set; }
        public decimal? TotalActualPrice { get; set; }
        public string InvoiceComments { get; set; }
        public string ActualUnitPriceUnit { get; set; }
    }
}
