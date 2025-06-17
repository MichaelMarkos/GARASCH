using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class AddNewSalesPricingDetailsData
    {
        public long SalesOfferId { get; set; }
        public List<GetTax> SalesOfferTaxList { get; set; }
        public List<ExtraCost> SalesOfferExtraCostList { get; set; }
        public List<GetSalesOfferDiscount> SalesOfferDiscountList { get; set; }
        public List<GetSalesOfferTermsAndConditions> SalesOfferTermsAndConditionsList { get; set; }
    }
}
