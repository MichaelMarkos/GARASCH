using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryItemMatrialReleaseListPagingFilters
    {
        [FromHeader]
        public long InventoryItemID { get; set; }
        [FromHeader]
        public long ToInventoryStoreID { get; set; }
        [FromHeader]
        public long ProjectID { get; set; }
        [FromHeader]
        public long ClientID { get; set; }
        [FromHeader]
        public long ToUserID { get; set; }
        [FromHeader]
        public long CreatorUserID { get; set; }
        [FromHeader]
        public DateTime? RequestDate { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 100;
        [FromHeader]
        public long ReleaseNo { get; set; }
        [FromHeader]
        public string Status { get; set; }

    }
}
