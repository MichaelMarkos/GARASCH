using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class MyOffersCRMDashboardResponse
    {
        public int TotalOffersCount { get; set; }
        public decimal TotalOffersPrice { get; set; }

        public int UnderPricingOffersCount { get; set; }
        public int UnderPricingDelayCount { get; set; }

        public int SendingOffersCount { get; set; }
        public decimal SendingOffersPrice { get; set; }

        public int WaitingApprovalCount { get; set; }
        public decimal WaitingApprovalPrice { get; set; }
        public int ApprovalDelayCount { get; set; }
        public int ApprovalWillExpireCount { get; set; }
        public int ApprovalExpiredCount { get; set; }

        public int ClosedOffersCount { get; set; }
        public decimal ClosedOffersPrice { get; set; }

        public int RejectedOffersCount { get; set; }
        public decimal RejectedOffersPrice { get; set; }

        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
