using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryStoreReports.UsedInResponse
{
    public class InventoryItemPhysicalBalance
    {
        public long InventoryItemId { get; set; }
        public string Comment { get; set; }
        public decimal? CurrentBalance { get; set; }
        public decimal? PhysicalBalance { get; set; }
        public List<long> IDSInventoryStoreItemList { get; set; }
    }
}
