using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetPurchasePOInvoiceDeductionTypeResponse
    {
        bool result;
        List<Error> errors;
        List<PurchasePOInvoiceDeductionTypeData> purchasePOInvoiceDeductionTypeList;



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
        public List<PurchasePOInvoiceDeductionTypeData> PurchasePOInvoiceDeductionTypeList
        {
            get
            {
                return purchasePOInvoiceDeductionTypeList;
            }

            set
            {
                purchasePOInvoiceDeductionTypeList = value;
            }
        }
    }
}
