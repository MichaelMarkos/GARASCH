using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryItemMatrialPagingFilters
    {
        [FromHeader]
        public long InventoryItemID { get; set; }
        [FromHeader]
        public long ProjectID { get; set; }
        [FromHeader]
        public long ClientID { get; set; }
        [FromHeader]
        public long InventoryStoreID { get; set; }
        [FromHeader]
        public long FromUserID { get; set; }
        [FromHeader]
        public long RequestNo { get; set; }
        [FromHeader]
        public long CreatorUserID { get; set; }
        [FromHeader]
        public long UserId { get; set; }
        [FromHeader]
        public long MatrialRequestTypeID { get; set; }
        [FromHeader]
        public long MatrialResponseTypeID { get;set; }
        [FromHeader]
        public int CurrentPage { get; set; }
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; }
        [FromHeader]
        public string Comment { get; set; }
        [FromHeader]
        public string Status { get; set; }
        [FromHeader]
        public DateTime? RequestDate { get; set; }
    }
}
