using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetOfferInventoryItemsFilters
    {
        [FromHeader]
        public long InventoryItemId { get; set; }
        [FromHeader]
        public int CategoryId { get; set; }
        [FromHeader]
        public int StoreId { get; set; }
        [FromHeader]
        public string InventoryItemName { get; set; } = "";
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; }
    }
}
