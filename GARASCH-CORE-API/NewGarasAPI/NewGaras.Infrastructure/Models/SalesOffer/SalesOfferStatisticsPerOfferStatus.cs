namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferStatisticsPerOfferStatus
    {
        public int TotalCountForPricingStatus { get; set; }
        public decimal? TotalCostForPricingStatus { get; set; }

        public int TotalCountForRecievedStatus { get; set; }
        public decimal? TotalCostForRecievedStatus { get; set; }

        public int TotalCountForClientApprovalStatus { get; set; }
        public decimal? TotalCostForClientApprovalStatus { get; set; }

        public int TotalCountForClosedStatus { get; set; }
        public decimal? TotalCostForClosedStatus { get; set; }

        public int TotalCountForRejectedStatus { get; set; }
        public decimal? TotalCostForRejectedStatus { get; set; }
    }
}