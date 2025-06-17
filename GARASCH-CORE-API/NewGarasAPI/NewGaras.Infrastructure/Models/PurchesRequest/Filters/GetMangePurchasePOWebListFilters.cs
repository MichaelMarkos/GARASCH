using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.Filters
{
    public class GetMangePurchasePOWebListFilters
    {
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public long? InventoryItemID { get; set; }
        [FromHeader]
        public long? CreatorUserID { get; set;}
        [FromHeader]
        public long? SupplierID { get; set; }
        [FromHeader]
        public long? POID { get; set; }
        [FromHeader]
        public long? POTypeID { get; set; }
        [FromHeader]
        public string RequestDate { get; set; }
        [FromHeader]
        public string CreationDate { get; set; }
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public string Status { get; set; }
        [FromHeader]
        public bool? HasAddingOrder { get; set; }
        [FromHeader]
        public bool? HasInvoice { get; set; }
        [FromHeader]
        public string SupplierInvoiceSerial { get; set; }
        [FromHeader]
        public bool? IsSentToAcc { get; set; }
        [FromHeader]
        public string SortByASC { get; set; }
        [FromHeader]
        public string SortByDESC { get; set; }
    }
}
