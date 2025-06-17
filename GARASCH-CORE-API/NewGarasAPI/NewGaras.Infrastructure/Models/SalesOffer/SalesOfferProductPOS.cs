using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferProductPOS
    {
        public long ProductID { get; set; }
        public string productComment { get; set; }
        public double? Quantity { get; set; }
        public decimal? ItemPrice { get; set; }
    }
}
