using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItemMatrialAddingAndExternalOrder.Filters
{
    public class GetInventoryItemMatrialAddingAndExternalOrderListFilters
    {
        [FromHeader]
        public long InventoryItemID { get; set; }
        [FromHeader]
        public long ToInventoryStoreID { get; set; }
        [FromHeader]
        public long FromSupplierID { get; set; }
        [FromHeader]
        public long CreatorUserID { get; set; }
        [FromHeader]
        public string SupplierItemSerial { get; set; }
        [FromHeader]
        public string OrderType { get; set; }
        [FromHeader]
        public string ReceiveDateFrom { get; set; }
        [FromHeader]
        public string ReceiveDateTo { get; set;}
    }
}
