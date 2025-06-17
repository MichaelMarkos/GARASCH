using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferAchievedTargetFilters
    {
        [FromHeader]
        public string OfferStatus { get; set; }
        [FromHeader]
        public string OfferType { get; set; }
        [FromHeader]
        public string ProductType { get; set; }
        [FromHeader]
        public string ClientName { get; set; }
        [FromHeader]
        public string ProjectName { get; set; }
        [FromHeader]
        public DateTime? From { get; set; }
        [FromHeader]
        public DateTime? To { get; set; }
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]
        public int BranchId { get; set; }

    }
}
