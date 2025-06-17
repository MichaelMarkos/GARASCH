namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientDashboard
    {
        public string FirstOfferDate { get; set; }
        public int OffersNumber { get; set; }
        public decimal TotalBusinessVolume { get; set; }
        public int OpenProjectsNumber { get; set; }
        public int ClosedProjectsNumber { get; set; }
        public int FollowUpCRMReportsNumber { get; set; }
        public int FollowUpSalesReportsNumber { get; set; }
        public bool IsExpired { get; set; }
        public string ExpirationDate { get; set; }
        public int ExpirationRemainingDays { get; set; }
    }
}