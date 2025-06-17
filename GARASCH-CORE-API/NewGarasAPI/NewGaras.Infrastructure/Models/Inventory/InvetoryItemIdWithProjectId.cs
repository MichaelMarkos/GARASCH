using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InvetoryItemIdWithProjectId
    {
        public long InventoryItemID { get; set; }
        public long? PojectID { get; set; }
        public string Comment { get; set; }
    }
}
