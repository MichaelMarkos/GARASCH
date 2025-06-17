using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class MatrialReleaseItem
    {
        public long Id { get; set; }
        public long MatrialReleaseId { get; set; }
        public string ItemName { get; set; }
        public string? ItemComment { get; set; }
        public long InventoryItemId{ get; set; }
        public int StoreId{ get; set; }
        public bool ReleaseReturned{ get; set; }
        public decimal? ReturnedRemain { get; set; }
        public decimal? Qty { get; set; }
        public long? ReleaseParentId { get; set; }
    }
}
