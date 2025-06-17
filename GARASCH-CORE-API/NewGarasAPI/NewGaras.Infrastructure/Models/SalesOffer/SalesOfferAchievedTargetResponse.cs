using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferAchievedTargetResponse
    {
        public bool Result { get; set; }
        public decimal? TotalOfferAmount { get; set; }
        public decimal? TotalFinalOfferPriceWithoutTax { get; set; }
        public decimal? TotalFinalOfferPrice {  get; set; }
        public int? CountOfSalesOffer { get; set; }
        public List<Error> Errors { get; set; }
    }
}
