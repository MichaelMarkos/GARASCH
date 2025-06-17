using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem.Filters
{
    public class AccountAndFinanceInventoryItemMovementListV2Filters
    {
        [FromHeader]
        public int CurrentPage { get; set; } = 0;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 1;
        [FromHeader]
        public long InventoryItemID { get; set; }
        [FromHeader]
        public string DateFrom { get; set; }
        [FromHeader]
        public string DateTo { get; set; }
        [FromHeader]
        public long? ClientId { get; set; }
        [FromHeader]
        public long? SupplierId { get; set; }
        [FromHeader]
        public long? PoId { get; set; }
        [FromHeader]
        public long? ProjectId { get; set; }
        [FromHeader]
        public string OperationType { get; set; }
        [FromHeader]
        public long? StoreID { get; set; }
    }
}
