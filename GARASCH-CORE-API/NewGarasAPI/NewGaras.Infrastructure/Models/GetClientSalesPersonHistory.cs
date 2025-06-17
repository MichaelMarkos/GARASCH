namespace NewGaras.Infrastructure.Models
{
    public class GetClientSalesPersonHistory
    {
        public long ID { get; set; }
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool Current { get; set; }
        public string ChangeReason { get; set; }
    }
}