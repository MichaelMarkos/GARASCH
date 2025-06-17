using NewGaras.Infrastructure.Models.Inventory.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class AddInventoryStoreWithMatrialRequest
    {
        public long? MatrialRequestId { get; set; }
        public long RequestTypeID { get; set; }
        public int InventoryStoreID { get; set; }
        public long FromUserId { get; set; }

        public string RequestDate { get; set; }
        public bool? IsFinish { get; set; }
        public string HoldReason { get; set; }

        public List<MatrialRequestItem> MatrialRequestItemList { get; set; }
    }
}
