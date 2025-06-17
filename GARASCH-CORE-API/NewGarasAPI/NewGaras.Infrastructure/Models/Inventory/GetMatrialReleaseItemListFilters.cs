using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetMatrialReleaseItemListFilters
    {
        [FromHeader]
        public long ProjectID { get; set; }
        [FromHeader]
        public long UserID { get; set; }
        [FromHeader]
        public int StoreID { get; set; }
        [FromHeader]
        public int StoreLocationID { get; set; }
        [FromHeader]
        public long InventoryItemID { get; set; }

        [FromHeader]
        public string Searchkey { get; set; }

        [FromHeader]
        public int CurrentPage { get; set; } = 1;

        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
    }
}
