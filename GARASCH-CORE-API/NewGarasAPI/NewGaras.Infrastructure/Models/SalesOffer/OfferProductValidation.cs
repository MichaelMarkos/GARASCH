using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class OfferProductValidation
    {
        public decimal ItemPrice{ get; set; }
        public decimal Quantity{ get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal PriceAfterTax { get; set; }

        public List<GetTax> OfferTaxes { get; set; }
    }
}
