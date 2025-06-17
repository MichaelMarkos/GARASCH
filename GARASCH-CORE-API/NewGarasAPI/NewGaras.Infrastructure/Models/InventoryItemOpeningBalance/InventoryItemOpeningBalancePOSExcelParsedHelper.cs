using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItemOpeningBalance
{
    public class InventoryItemOpeningBalancePOSExcelParsedHelper
    {
        public long SupplierID { get; set; }

        public long InventoryItemID { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceUOR { get; set; }
        public int CurrencyId { get; set; }
        public decimal RateToLocal { get; set; }
        public string Serial { get; set; }
        public DateTime? ExpDate { get; set; }
        public DateTime? RecevingData { get; set; }
        public int InventoryStoreID { get; set; }

        public int UORQYT { get; set; }
        public long UORUnit { get; set; }

        public int UOPQYT { get; set; }
        public long UOPUnit { get; set; }

        public decimal ConvRate { get; set; }

        public decimal customPrice { get; set; }
        public decimal? Price1 { get; set; }
        public decimal? Price2 { get; set; }
        public decimal? Price3 { get; set; }
        public decimal? Cost { get; set; }
    }
}
