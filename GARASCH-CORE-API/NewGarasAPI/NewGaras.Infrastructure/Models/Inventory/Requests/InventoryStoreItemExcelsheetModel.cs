using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class InventoryStoreItemExcelsheetModel
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public List<InventoryStoreVM> InventoryStoreList { get; set; }
    }
}
