using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryCategoryStoreItemResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public List<InventoryItemCategoryVM> InventoryItemCategoryList { get; set; }

        public InventoryItemCategoryOBJ InventoryItemCategorySumm { get; set; }
    }
}
