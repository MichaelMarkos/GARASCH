using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse
{
    public class InventoryItemSupplier
    {
        public string SupplierName { get; set; }
        public string Serial {  get; set; }
        public string QTY { get; set; }
        public string OrderingNo { get; set; }
        public string ExpDate { get; set; }
        public string OperationType { get; set; }
    }
}
