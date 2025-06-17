using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryStoreItemMovementReportResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public string Message { get; set; }
        public List<InventoryItemStoreMovementVM> InventoryItemStoreMovementList { get; set; }
    }
}
