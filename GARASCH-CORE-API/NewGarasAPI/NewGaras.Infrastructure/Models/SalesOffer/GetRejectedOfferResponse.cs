using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.SalesOffer.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    [DataContract]
    public class GetRejectedOfferResponse
    {
        List<PrOffer> prOffers;
        PrSupplierOffer prSupplierOffer;
        List<PrSupplierOfferItem> prSupplierOfferItems;

        bool result;
        List<Error> errors;


        [DataMember]
        public List<PrOffer> PrOffers
        {
            get
            {
                return prOffers;
            }

            set
            {
                prOffers = value;
            }
        }
        [DataMember]
        public PrSupplierOffer PrSupplierOffer
        {
            get
            {
                return prSupplierOffer;
            }

            set
            {
                prSupplierOffer = value;
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
