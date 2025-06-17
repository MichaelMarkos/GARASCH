using NewGaras.Infrastructure.Models.Inventory.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryInternalTransferItemForWebList
    {
        public List<InventoryInternalTransferOrderInfo> InternalTransferOrderList { get; set; }
    }
}
