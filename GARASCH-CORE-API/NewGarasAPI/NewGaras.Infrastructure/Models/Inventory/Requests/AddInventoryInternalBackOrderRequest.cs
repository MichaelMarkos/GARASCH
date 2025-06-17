using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class AddInventoryInternalBackOrderRequest
    {
        public string OperationType {  get; set; }
        public long FromUserID { get; set; }
        public int InventoryStoreID { get; set; }
        public string RecivingDate { get; set; }
        public bool? Cutoday { get; set; }

        public List<InternalBackOrderItem> InternalBackOrderItemList { get; set; }
    }
}
