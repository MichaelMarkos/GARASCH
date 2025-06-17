using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetPurchasePOInvoiceNotIncludeTaxTypeResponse
    {
        bool result;
        List<Error> errors;
        List<PurchasePOInvoiceNotIncludeTaxTypeData> purchasePOInvoiceNotIncludeTaxTypeList;



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
        public List<PurchasePOInvoiceNotIncludeTaxTypeData> PurchasePOInvoiceNotIncludeTaxTypeList
        {
            get
            {
                return purchasePOInvoiceNotIncludeTaxTypeList;
            }

            set
            {
                purchasePOInvoiceNotIncludeTaxTypeList = value;
            }
        }

    }
}
