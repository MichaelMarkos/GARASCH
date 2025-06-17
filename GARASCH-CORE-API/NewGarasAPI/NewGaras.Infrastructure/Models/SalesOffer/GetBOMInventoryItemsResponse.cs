using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetBOMInventoryItemsResponse
    {
        public decimal BOMCurrentPrice { get; set; }
        public List<BOMPartitionItemInfo> BomInventoryItemList { get; set; }
        public List<BomPrices> BomPricesList { get; set; }
        public bool Result {  get; set; } 
        public List<Error> Errors { get; set; }
    }
}
