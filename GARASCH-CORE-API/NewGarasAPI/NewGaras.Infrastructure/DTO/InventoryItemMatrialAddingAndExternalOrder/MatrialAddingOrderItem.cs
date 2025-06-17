using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.InventoryItemMatrialAddingAndExternalOrder
{
    public class MatrialAddingOrderItem
    {
        public long InventoryItemID { get; set; }
        public long POID { get; set; }
        public decimal QTYUOR { get; set; }
        public decimal? QTYUOP { get; set; }
        public string ExpData { get; set; }
        public string Serial { get; set; }
        public long? QIReport { get; set; }
        public string Comment { get; set; }
        public int? StoreLocationID { get; set; }
        public long? ParentInventoryStoreItem { get; set; }
    }
}
