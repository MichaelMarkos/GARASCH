using NewGaras.Infrastructure.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class GetInventoryItemCategoryResponse
    {
        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
        public List<InventoryItemCategoryData> InventoryItemCategoryList { get; set; }
        public List<TreeViewDto2> GetInventoryItemCategoryList { get; set; }
    }
}
