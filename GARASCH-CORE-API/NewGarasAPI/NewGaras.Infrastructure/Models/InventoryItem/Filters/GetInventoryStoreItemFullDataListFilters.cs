using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem.Filters
{
    public class GetInventoryStoreItemFullDataListFilters
    {
        [FromHeader]
        public int? InventoryStoreID { get; set; }
        [FromHeader]
        public int? InventoryItemCategoryID { get; set; }
        [FromHeader]
        public int? PriorityID { get; set; }
        [FromHeader]
        public long? InventoryItemID { get; set; }
        [FromHeader]
        public long? SupplierID { get; set; }
        [FromHeader]
        public string OperationType { get; set; }
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public bool? NotPricidBefore { get; set; }
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 50;
        [FromHeader]
        public int CurrentPage { get; set; } = 1;

    }
}
