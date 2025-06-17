using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryInternalTransferOrderItemInfoResponse
    {
        public InventoryItemInternalTransferOrderInfo InventoryItemInfo { get; set; }
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
