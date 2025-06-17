using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse
{
    public class InventoryStoreItemHoldQTY
    {
        public List<long> InventoryStoreItemIDsList { get; set; }
        public decimal RemainHoldQTY { get; set; }
        public string Serial { get; set; }
        public string ExpDate { get; set; }
    }
}
