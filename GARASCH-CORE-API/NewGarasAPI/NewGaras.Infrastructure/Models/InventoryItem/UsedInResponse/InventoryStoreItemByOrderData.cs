using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse
{
    public class InventoryStoreItemByOrderData
    {
        public long ID { get; set; }
        public decimal Balance { get; set; }
        public string ExpDate { get; set; }
        public string ItemSerial { get; set; }
        public decimal FinalBalance { get; set; }
        public decimal? AddingFromPOId { get; set; }
        public decimal? POInvoiceTotalPriceEGP { get; set; }
        public decimal? POInvoiceTotalCostEGP { get; set; }
        public string ItemName { get; set; }
    }
}
