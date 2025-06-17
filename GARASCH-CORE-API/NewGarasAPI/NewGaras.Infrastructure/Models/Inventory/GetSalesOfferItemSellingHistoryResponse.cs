using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetSalesOfferItemSellingHistoryResponse
    {
        List<SalesOfferItemSalesDetails> salesOfferItemsSalesDetailsList;
        bool result;
        List<Error> errors;

        [DataMember]
        public List<SalesOfferItemSalesDetails> SalesOfferItemsSalesDetailsList
        {
            get
            {
                return salesOfferItemsSalesDetailsList;
            }

            set
            {
                salesOfferItemsSalesDetailsList = value;
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
