using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryStoreItemBatchWithExpDateFilters
    {
        [FromHeader]
        public long InventoryItemID { get; set; }
        [FromHeader]
        public long StorID { get; set; }
        [FromHeader]
        public long ReportID { get; set; }
        [FromHeader]
        public int StorLocationID { get; set; }
        [FromHeader]
        public string Serial { get; set;}
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 1000;
        [FromHeader]
        public DateTime? ExpDate { get; set;}
        [FromHeader]
        public string SortByASC { get; set; }
        [FromHeader]
        public string SortByDESC { get; set;}
    }
}
