using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    public class PurchaseForStoreHelper
    {
        public long InventoryItemID { get; set; }
        public string InventoryItemName { get; set; }
        public long InventoryStoreID { get; set; }
        public string InventoryStoreName { get; set; }
        public decimal? ItemPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public string AddedDate { get; set; }
        public long SupplierID { get; set; }
        public string SupplierName { get; set; }

    }
}
