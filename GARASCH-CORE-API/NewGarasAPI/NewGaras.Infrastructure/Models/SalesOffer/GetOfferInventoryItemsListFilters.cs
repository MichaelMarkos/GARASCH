using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetOfferInventoryItemsListFilters
    {
        [FromHeader]
        public long InventoryItemId { get; set; }
        [FromHeader]
        public int CategoryId { get; set; }
        [FromHeader]
        public int StoreId { get; set; }
        [FromHeader]
        public string InventoryItemName { get; set; }
        [FromHeader]
        public bool? IsActive { get; set; }
        [FromHeader]
        public bool? IsFinalProduct { get; set; }
        [FromHeader]
        public bool? IsRentItem { get; set; }
        [FromHeader]
        public bool? IsAsset { get; set; }
        [FromHeader]
        public bool? IsNonStock { get; set; }

    }
}
