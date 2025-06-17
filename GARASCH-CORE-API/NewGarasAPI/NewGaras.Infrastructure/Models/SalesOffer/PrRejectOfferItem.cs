using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class PrRejectOfferItem
    {
        public long Id { get; set; }
        public string SupplierName { get; set; }
        public decimal? TotalEstimatedCost { get; set; }
        public string CurrencyName { get; set; }
        public decimal? RateToEGP { get; set; }

    }
}
