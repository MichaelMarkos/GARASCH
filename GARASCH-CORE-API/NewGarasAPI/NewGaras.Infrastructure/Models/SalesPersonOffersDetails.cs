namespace NewGaras.Infrastructure.Models
{
    public class SalesPersonOffersDetails
    {
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }

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
    }
}