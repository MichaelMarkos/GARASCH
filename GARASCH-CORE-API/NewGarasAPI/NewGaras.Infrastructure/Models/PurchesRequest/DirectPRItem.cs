using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest
{
    public class DirectPRItem
    {
        public long InventoryItemID {  get; set; }
        public decimal ReqQTY { get; set; }
        public string Comment { get; set; }
        public string DirectPrNotes { get; set; }
    }
}
