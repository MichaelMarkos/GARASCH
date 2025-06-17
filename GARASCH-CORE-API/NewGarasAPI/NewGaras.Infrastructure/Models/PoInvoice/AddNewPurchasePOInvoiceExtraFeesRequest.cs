using NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PoInvoice
{
    [DataContract]
    public class AddNewPurchasePOInvoiceExtraFeesRequest
    {
        List<PurchasePOExtraFees> purchasePOExtraFeesList;

        [DataMember]
        public List<PurchasePOExtraFees> PurchasePOExtraFeesList
        {
            get
            {
                return purchasePOExtraFeesList;
            }

            set
            {
                purchasePOExtraFeesList = value;
            }
        }

    }
}
