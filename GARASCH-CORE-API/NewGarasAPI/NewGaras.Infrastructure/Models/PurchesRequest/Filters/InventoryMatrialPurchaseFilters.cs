using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.Filters
{
    public class InventoryMatrialPurchaseFilters
    {
        [FromHeader]
        public long? InventoryItemID { get; set; }
        [FromHeader]
        public long? PurchaseRequestID { get; set; }
        [FromHeader]
        public bool? IsDirectPR {  get; set; }
        [FromHeader]
        public long? FormInventoryStoreID { get; set; }
        [FromHeader]
        public long? CreatorUserID { get; set; }
        [FromHeader]
        public string Status { get; set; }
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public string RequestDate { get; set; }
        [FromHeader]
        public int currentpage { get; set; } = 1;
        [FromHeader]
        public int ItemsPerPage { get; set; } = 10;
    }
}
