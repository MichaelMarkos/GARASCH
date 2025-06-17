using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse
{
    public class LocationBalance
    {
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public decimal Balance { get; set; }
    }
}
