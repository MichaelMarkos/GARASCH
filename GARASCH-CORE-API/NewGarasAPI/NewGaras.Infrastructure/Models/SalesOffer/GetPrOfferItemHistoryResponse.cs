using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    [DataContract]
    public class GetPrOfferItemHistoryResponse
    {
        List<PrSupplierOfferItem> prSupplierOfferItemHistory;

        bool result;
        List<Error> errors;

        [DataMember]
        public List<PrSupplierOfferItem> PrSupplierOfferItemHistory
        {
            get
            {
                return prSupplierOfferItemHistory;
            }

            set
            {
                prSupplierOfferItemHistory = value;
            }
        }

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
    }
}
