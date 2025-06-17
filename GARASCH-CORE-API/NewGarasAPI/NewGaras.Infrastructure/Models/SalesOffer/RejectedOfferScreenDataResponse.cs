using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    [DataContract]
    public class RejectedOfferScreenDataResponse
    {
        List<SelectDDL> currencyDDL;
        List<SelectDDL> suppliersDDL;
        List<PrSupplierOfferItem> prSupplierOfferItems;

        bool result;
        List<Error> errors;


        [DataMember]
        public List<SelectDDL> CurrencyDDL
        {
            get
            {
                return currencyDDL;
            }

            set
            {
                currencyDDL = value;
            }
        }

        [DataMember]
        public List<SelectDDL> SuppliersDDL
        {
            get
            {
                return suppliersDDL;
            }

            set
            {
                suppliersDDL = value;
            }
        }
        [DataMember]
        public List<PrSupplierOfferItem> PrSupplierOfferItems
        {
            get
            {
                return prSupplierOfferItems;
            }

            set
            {
                prSupplierOfferItems = value;
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
