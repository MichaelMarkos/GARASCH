using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryStoreItemView
    {
        public long ID { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string PartNO { get; set; }
        public string InventoryStoreName { get; set; }
        public string Category { get; set; }
        public decimal UnitCost { get; set; }

        public string ItemSerialCounter { get; set; }
        public bool Active { get; set; }
        public decimal? Cost1 { get; set; }
        public decimal? Cost2 { get; set; }
        public decimal? Cost3 { get; set; }
    }
}
