using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryItemStockBalanceHoldResponse
    {
        public List<InventoryStoreHoldItemByStore> InventoryStoreHoldItemByStoreList {  get; set; }
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public decimal TotalItemHoldStock {  get; set; }
        public string UOR { get; set; }
        
    }
}
