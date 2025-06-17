using NewGarasAPI.Models.AccountAndFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryStoreItemTotalPriceResponse
    {
        public InventoryStoreItemTotalPrice Data { get; set; } = new InventoryStoreItemTotalPrice();
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
