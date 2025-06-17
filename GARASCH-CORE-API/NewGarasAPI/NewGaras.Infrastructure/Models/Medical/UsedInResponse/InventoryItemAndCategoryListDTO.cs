using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical.UsedInResponse
{
    public class InventoryItemAndCategoryListDTO
    {
        public long InventoryItemId { get; set; }
        public int InventoryItemCategoryID { get; set; }
        public decimal ItemPrice { get; set; }
        public int Quantity { get; set; }
    }
}
