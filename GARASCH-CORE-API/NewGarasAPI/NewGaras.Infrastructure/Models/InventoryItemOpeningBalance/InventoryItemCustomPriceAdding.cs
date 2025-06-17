using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItemOpeningBalance
{
    public class InventoryItemCustomPriceAdding
    {
        public long InventoryItemID { get; set; }
        public decimal customPrice { get; set; }
        public decimal? Price1 { get; set; }
        public decimal? Price2 { get; set; }
        public decimal? Price3 { get; set; }
        public decimal? Cost { get; set; }
    }
}
