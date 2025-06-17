using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryStoreReports.UsedInResponse
{
    public class InventoryStoreReportItem
    {
        public long ID { get; set; }
        public long InventoryItemID { get; set; }
        public string ItemName { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal PhysicalBalance { get; set; }
        public decimal UnitCost { get; set; }
        public string Comment { get; set; }
        public bool? IsFinished { get; set; }
    }

}
