using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryItemInfoForPOS
    {
        public long? ID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string Category {  get; set; }
        public int? CategoryId { get; set; }
        public string PurchasingUnit { get; set; }
        public string RequestionUnit { get; set; }
        public decimal? ConvertRateFromPurchasingToRequestionUnit { get; set; }
        public decimal? CustomPrice { get; set; }
        public string ItemImage {  get; set; }
        public decimal? Cost1 { get; set; }
        public decimal? Cost2 { get; set; }
        public decimal? Cost3 { get; set; }
        public string RequestionUOMShortName { get; set; }
        public decimal Balance { get; set; }
        public long? InventoryStoreItemId { get; set; }
    }
}
