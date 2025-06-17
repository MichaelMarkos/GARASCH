using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class AddOneInventoryStoreItemPricing
    {
        public long? IDSinventoryItem {  get; set; }
        public decimal? Custom {  get; set; }
        public decimal? Price1 { get; set; }
        public decimal? Price2 { get; set; }
        public decimal? Price3 { get; set; }
    }
}
