using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem.Filters
{
    public class GetAccountAndFinanceInventoryStoreItemReportListFilters
    {
        [FromHeader]
        public long? InventoryStoreID { get; set; }
        [FromHeader]
        public decimal? ExceedBalance { get; set; }
        [FromHeader]
        public decimal? LowStock { get; set; }
        [FromHeader]
        public string ItemSerial { get; set; }
        [FromHeader]
        public bool? IsExpDate { get; set; }
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public string MatrialAddingOrderSerial { get; set; }
        [FromHeader]
        public string ChapterName { get; set; }
        [FromHeader]
        public DateTime? NotReleasedFrom { get; set; }
        [FromHeader]
        public string SortBy { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
    }
}
