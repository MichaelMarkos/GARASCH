namespace NewGaras.Infrastructure.Models
{
    public class EngagingRate
    {
        public decimal TotalEngagingRate { get; set; }
        public decimal TotalDealsExtraCosts { get; set; }
        public decimal TotalEngagingRateLastYear { get; set; }
        public string EngagingRatePercentage { get; set; }
        public string ClosingRatePercentage { get; set; }
        public string EngagingRateState { get; set; }
        public int ClientsCount { get; set; }
        public int ClientsRFQCount { get; set; }
        public int DealedClientsCount { get; set; }
        public int ClientsCountLastYear { get; set; }
        public int DealsCount { get; set; }
        public int DealsCountLastYear { get; set; }
    }
}