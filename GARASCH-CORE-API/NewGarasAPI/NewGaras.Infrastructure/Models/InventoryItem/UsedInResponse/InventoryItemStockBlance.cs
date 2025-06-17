using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse
{
    public class InventoryItemStockBlance
    {
        public int No { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string Comment { get; set; }
        public decimal Balance { get; set; }
        public List<LocationBalance> LocationBalance { get; set; }
        public List<string> ListOfComments { get; set; }

    }
}
