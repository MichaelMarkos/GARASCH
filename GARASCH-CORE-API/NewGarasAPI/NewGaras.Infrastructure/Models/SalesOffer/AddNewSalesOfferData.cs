using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class AddNewSalesOfferData
    {
        GetSalesOffer salesOffer;
        List<GetSalesOfferProduct> salesOfferProductList;
        List<GetTax> salesOfferTaxList;
        List<ExtraCost> salesOfferExtraCostList;
        List<Attachment> salesOfferAttachmentList;

        [DataMember]
        public GetSalesOffer SalesOffer
        {
            get
            {
                return salesOffer;
            }

            set
            {
                salesOffer = value;
            }
        }

        [DataMember]
        public List<GetSalesOfferProduct> SalesOfferProductList
        {
            get
            {
                return salesOfferProductList;
            }

            set
            {
                salesOfferProductList = value;
            }
        }

        [DataMember]
        public List<GetTax> SalesOfferTaxList
        {
            get
            {
                return salesOfferTaxList;
            }

            set
            {
                salesOfferTaxList = value;
            }
        }

        [DataMember]
        public List<ExtraCost> SalesOfferExtraCostList
        {
            get
            {
                return salesOfferExtraCostList;
            }

            set
            {
                salesOfferExtraCostList = value;
            }
        }

        [DataMember]
        public List<Attachment> SalesOfferAttachmentList
        {
            get
            {
                return salesOfferAttachmentList;
            }

            set
            {
                salesOfferAttachmentList = value;
            }
        }
    }
}
