using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class STP_EINVOICE_GetInvoiceLines_Result
    {
        public Nullable<double> Quantity { get; set; }
        public Nullable<decimal> ItemPrice { get; set; }
        public Nullable<decimal> DiscountPercentage { get; set; }
        public Nullable<double> salesTotal { get; set; }
        public Nullable<double> DiscountValue { get; set; }
        public Nullable<double> netTotal { get; set; }
        public string ItemTypeEINVOICE { get; set; }
        public string internalCode { get; set; }
        public string ItemCode { get; set; }
        public string UOM_CODE { get; set; }
        public long SalesOfferProductID { get; set; }
        public Nullable<long> itemID { get; set; }
    }
}
