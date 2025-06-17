using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryInternalTransferFilters
    {
        [FromHeader]
        public long InventoryItemID { get; set; }
        [FromHeader]
        public long ToInventoryStoreID { get; set; }
        [FromHeader]
        public long FormInventoryStoreID { get; set;}
        [FromHeader]
        public long CreatorUserID { get;set;}
        [FromHeader]
        public DateTime? ReceiveDate { get;set;}
        [FromHeader]
        public DateTime? CreationDate { get; set; }
    }
}
