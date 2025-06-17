using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ItemsPricing
{
    public class InventoryStoreItemFullData  : InventoryStoreItem
    {
        public decimal CustomeUnitPrice { get; set; }
        public decimal AverageUnitPrice { get; set; }
        public decimal MaxUnitPrice { get; set; }

        public decimal LastUnitPrice { get; set; }
        public decimal? Balance { get; set; }
    }
}
