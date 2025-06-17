using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class AddInventoryInternalTransferOrderRequest
    {
        public int FromInventoryStorID { get; set; }
        public int ToInventoryStorID { get; set; }
        public string ReceivingDate { get; set; }

        public List<InternalTransferOrderItem> InternalTransferOrderItemList { get; set; }
    }

    public class AddInventoryInternalTransferOrderItemsRequest
    {
        public long TransferOrderId { get; set; }

        public List<InternalTransferOrderItem> InternalTransferOrderItemList { get; set; }
    }
}
