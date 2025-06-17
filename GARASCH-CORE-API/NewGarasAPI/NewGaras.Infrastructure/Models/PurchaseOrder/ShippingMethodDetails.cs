using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    public class ShippingMethodDetails
    {
        long shippingMethodID;
        decimal? fees;
        int? currencyId;

        [FromBody]
        public long ShippingMethodID
        {
            get { return shippingMethodID; }
            set { shippingMethodID = value; }
        }
        [FromBody]
        public decimal? Fees
        {
            get { return fees; }
            set { fees = value; }
        }
        [FromBody]
        public int? CurrencyID
        {
            get { return currencyId; }
            set { currencyId = value; }
        }
    }

}
