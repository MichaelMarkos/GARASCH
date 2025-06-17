using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryCategoryPerItemData
    {
        public int? ID { get; set; }
        public int? ParentCategoryID { get; set; }
        public int? CategoryTypeId { get; set; }
        public string CategoryTypeName { get; set; }
        public bool? HaveItem { get; set; }
        public bool? IsFinalProduct { get; set; }
        public bool? IsRentItem { get; set; }
        public bool? IsAsset { get; set; }
        public bool? IsNonStock { get; set; }
        public bool? HasChild { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParentName { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
    }
}
