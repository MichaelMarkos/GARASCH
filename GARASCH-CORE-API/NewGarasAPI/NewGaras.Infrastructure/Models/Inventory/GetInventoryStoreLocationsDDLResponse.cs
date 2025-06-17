using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryStoreLocationsDDLResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public List<InventoryStoreLocationDDLData> InventoryStoreLocationDDLList { get; set; }
    }
}
