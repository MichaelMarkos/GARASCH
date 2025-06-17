using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryKeeperData
    {
        public int ID { get; set; }
        public int InventoryStoreID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Location { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }

    }
}
