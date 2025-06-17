namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetContractDetails
    {
        public long ManagementMaintenanctOrderID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public decimal? Cost { get; set; }
        public int RemainNoVisit { get; set; }
    }
}