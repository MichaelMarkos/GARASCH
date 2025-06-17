using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesOfferDashboardResponse
    {
        public SalesOfferTypeDetails UnderPricingOffersList {  get; set; }
        public SalesOfferTypeDetails SendingOfferToClientOffersList { get; set; }
        public SalesOfferTypeDetails WaitingClientApprovalOffersList { get; set; }
        public SalesOfferTypeDetails ClosedOffersList { get; set; }
        public SalesOfferTypeDetails RejectedOffersList { get; set; }

        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
