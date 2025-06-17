using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItemOpeningBalance
{
    public class InventoryItemOpeningBalancePOSExcelHelper
    {
        public string SupplierID { get; set; }

        public string InventoryItemID { get; set; }
        public string UnitPrice { get; set; }
        public string UnitPriceUOR { get; set; }
        public string CurrencyId { get; set; }
        public string RateToLocal { get; set; }
        public string Serial { get; set; }
        public string ExpDate { get; set; }
        public string RecevingData { get; set; }
        public string InventoryStoreID { get; set; }


        public string UORQYT { get; set; }
        public string UORUnit {  get; set; }

        public string UOPQYT { get; set; }
        public string UOPUnit { get; set; }

        public string ConvRate { get; set; }        //not used , take Exhange factor instead of it

        public string customPrice { get; set; }
        public string Price1 { get; set; }
        public string Price2 { get; set; }
        public string Price3 { get; set; }

        //--------------item cost------------
        public string Cost { get; set; }
    }
}
