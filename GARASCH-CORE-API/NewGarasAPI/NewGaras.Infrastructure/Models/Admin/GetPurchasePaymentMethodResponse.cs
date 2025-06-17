using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetPurchasePaymentMethodResponse
    {
        bool result;
        List<Error> errors;
        List<PurchasePaymentMethodData> purchasePaymentMethodList;



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
        public List<PurchasePaymentMethodData> PurchasePaymentMethodList
        {
            get
            {
                return purchasePaymentMethodList;
            }

            set
            {
                purchasePaymentMethodList = value;
            }
        }
    }
}
