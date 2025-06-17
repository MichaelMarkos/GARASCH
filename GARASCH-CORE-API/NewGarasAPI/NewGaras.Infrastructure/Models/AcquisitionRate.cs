namespace NewGaras.Infrastructure.Models
{
    public class AcquisitionRate
    {
        public decimal TotalAcquisitionRate { get; set; }
        public decimal TotalDealsExtraCosts { get; set; }
        public decimal TotalAcquisitionRateLastYear { get; set; }
        public string AcquisitionRatePercentage { get; set; }
        public string ClosingRatePercentage { get; set; }
        public string AcquisitionRateState { get; set; }
        public int ClientsRFQCount { get; set; }
        public int ClientsCount { get; set; }
        public int DealedClientsCount { get; set; }
        public int ClientsCountLastYear { get; set; }
        public int DealsCount { get; set; }
        public int DealsCountLastYear { get; set; }
    }
}