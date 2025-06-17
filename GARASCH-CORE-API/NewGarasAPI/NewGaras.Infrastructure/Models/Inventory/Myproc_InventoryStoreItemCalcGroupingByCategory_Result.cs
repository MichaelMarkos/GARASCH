using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class Myproc_InventoryStoreItemCalcGroupingByCategory_Result
    {
        public int? InventoryItemCategoryID { get; set; }
        public decimal? POInvTotalCost { get; set; }
        public decimal? POInvTotalCostWithRate { get; set; }
    }
}
