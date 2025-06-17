using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryItemListFilters
    {
        [FromHeader]
        public long StoreId { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 1000000;
        [FromHeader]
        public decimal MinBalance { get; set; }
        [FromHeader]
        public decimal MaxBalance { get; set; }
        [FromHeader]
        public bool GetNotActive { get; set; }
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public string MarketName { get; set; }
        [FromHeader]
        public string CommercialName { get; set; }
        [FromHeader]
        public string PartNo { get; set; }
        [FromHeader]
        public string ChapterName { get; set; }
        [FromHeader]
        public string SearchCode { get; set; }
        [FromHeader]
        public long PriorityID { get; set; }
        [FromHeader]
        public long InventoryItemCategoryID { get; set; }
        [FromHeader]
        public string Type { get; set; }
        [FromHeader]
        public bool? HaveOpeningBalance { get; set; }
        [FromHeader]
        public DateTime? NotReleasedFrom { get; set; }
        [FromHeader]
        public DateTime? NotReleasedTo { get; set; }
        [FromHeader]
        public long HeadParentCategoryId {  get; set; }
    }
}
