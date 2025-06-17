using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryItemCategoryListFilters
    {
        [FromHeader]
        public bool? IsRentItem { get; set; } = null;
        [FromHeader]
        public bool? IsAsset { get; set; } = null;
        [FromHeader]
        public bool? IsNonStock { get; set; } = null;
        [FromHeader]
        public bool? IsFinalProduct { get; set; } = null;
        [FromHeader]
        public long StoreId { get; set; } = 0;
    }
}
