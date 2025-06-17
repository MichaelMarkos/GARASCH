using NewGaras.Infrastructure.Models.SalesOffer.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    [DataContract]
    public class AddEditSupplierOfferResponse
    {
        PrSupplierOffer prSupplierOffer;
        List<PrSupplierOfferItem> prSupplierOfferItems;

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
    }
}
