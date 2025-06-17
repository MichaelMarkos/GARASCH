using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryItemMovementListV2Filters
    {
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public long InventoryItemID { get; set; }
        [FromHeader]
        public DateTime? ToDate { get; set; }
        [FromHeader]
        public DateTime? FromDate { get; set; }
        [FromHeader]
        public long ClientId { get; set; }
        [FromHeader]
        public long SupplierId { get; set; }
        [FromHeader]
        public long PoId { get; set; }
        [FromHeader]
        public long ProjectId { get; set; }
        [FromHeader]
        public string OperationType { get; set; }
    }
}
