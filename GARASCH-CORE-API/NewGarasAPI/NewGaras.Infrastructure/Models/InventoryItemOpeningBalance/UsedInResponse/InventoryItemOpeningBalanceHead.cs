using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.UsedInResponse
{
    
    public class InventoryItemOpeningBalanceHead
    {

        public int InventoryStoreID { set; get; }
        public string RecevingData { set; get; }
        public List<InventoryItemOpeningBalance> ItemList { set; get; }

        // For POS 
        public long? SupplierId { set; get; }
    }
}
