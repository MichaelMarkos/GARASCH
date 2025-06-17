using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.Filters
{
    public class GetRejectedOfferFilters
    {
        [FromHeader]
        public long? SupplierOfferId { get; set; }
        [FromHeader]
        public long? PoId { get; set; }
        [FromHeader]
        public long? PrId { get; set; }
        [FromHeader]
        public string Status { get; set; }
    }
}
