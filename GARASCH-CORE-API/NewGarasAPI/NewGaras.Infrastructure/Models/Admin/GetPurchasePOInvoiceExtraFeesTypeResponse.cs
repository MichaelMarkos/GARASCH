using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetPurchasePOInvoiceExtraFeesTypeResponse
    {
        bool result;
        List<Error> errors;
        List<PurchasePOInvoiceExtraFeesTypeData> purchasePOInvoiceExtraFeesTypeList;



        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }



        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }

        [DataMember]
        public List<PurchasePOInvoiceExtraFeesTypeData> PurchasePOInvoiceExtraFeesTypeList
        {
            get
            {
                return purchasePOInvoiceExtraFeesTypeList;
            }

            set
            {
                purchasePOInvoiceExtraFeesTypeList = value;
            }
        }
    }
}
