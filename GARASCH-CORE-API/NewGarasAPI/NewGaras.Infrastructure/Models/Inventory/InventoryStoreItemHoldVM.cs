using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryStoreItemHoldVM
    {
        public long InventoryItemID { get; set; }
        public string ItemName { get; set; }
        public string Code { get; set; }
        public string Serial { get; set; }
        public string ExpDate { get; set; }
        public decimal? HoldQTY { get; set; }
    }
}
