using NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.Responses
{
    [DataContract]
    public class GetPurchasePOResponse
    {
        List<PurchasePOByDate> purchasePOByDateList;
        bool result;
        List<Error> errors;

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
        public List<PurchasePOByDate> PurchasePOByDateList
        {
            get
            {
                return purchasePOByDateList;
            }

            set
            {
                purchasePOByDateList = value;
            }
        }


    }
}
