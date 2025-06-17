using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryItemMatrialFilters
    {
        [FromHeader]
        public long InventoryItemID { get; set; }
        [FromHeader]
        public long InventoryStoreID { get; set; }
        [FromHeader]
        public long FromUserID { get; set; }
        [FromHeader]
        public long CreatorUserID { get; set; }
        [FromHeader]
        public long MatrialRequestTypeID { get; set; }
        [FromHeader]
        public DateTime? RequestDate { get; set; }

    }
}
