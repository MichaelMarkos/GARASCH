using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetPurchasePOInvoiceTaxIncludedTypeResponse
    {
        bool result;
        List<Error> errors;
        List<PurchasePOInvoiceTaxIncludedTypeData> purchasePOInvoiceTaxIncludedTypeList;



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
        public List<PurchasePOInvoiceTaxIncludedTypeData> PurchasePOInvoiceTaxIncludedTypeList
        {
            get
            {
                return purchasePOInvoiceTaxIncludedTypeList;
            }

            set
            {
                purchasePOInvoiceTaxIncludedTypeList = value;
            }
        }

    }
}
