using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.UsedInResponse
{
    public class InventoryItemOpeningBalance
    {
        public long InventoryItemID { set; get; }
        public decimal QTY { set; get; }
        public string ExpData { set; get; }
        public string Serial { set; get; }
        // Comment on LIBMARK this Public Id for Library
        public int? PublicId { set; get; }
        public string Comment { set; get; }
        public int? StoreLocationID { set; get; }
        public decimal? CostEGP { set; get; }
        public int? CurrencyId { set; get; }
        public decimal? RateToEGP { set; get; }
    }
}
