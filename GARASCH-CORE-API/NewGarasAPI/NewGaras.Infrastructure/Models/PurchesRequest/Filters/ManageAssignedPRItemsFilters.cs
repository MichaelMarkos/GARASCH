using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.Filters
{
    public class ManageAssignedPRItemsFilters
    {
        [FromHeader]
        public long? UserAssignedTo { get; set; }
        [FromHeader]
        public long? InventoryItemID { get; set; }
        [FromHeader]
        public bool? IsDirectPR { get; set; }
        [FromHeader]
        public long? FormInventoryStoreID { get; set; }
        [FromHeader]
        public long? ProjectID { get; set; }
    }
}
