using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventortyStoreIncludeLocation
    {
        public int Id { get; set; }
        public string StoreName { get; set; }
        public string? LocationName { get; set; }
        public int? CountOfKeepers { get; set; }
    }
}
