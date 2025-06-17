using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class MatrialRequestItemModel
    {
        public long Id { get; set; }
        public string ItemName { get; set; }
        public string? ItemComment { get; set; }
        public long InventoryItemId { get; set; }
        public decimal? Qty { get; set; }
    }
}
