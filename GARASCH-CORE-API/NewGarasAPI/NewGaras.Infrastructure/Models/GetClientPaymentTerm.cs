namespace NewGaras.Infrastructure.Models
{
    public class GetClientPaymentTerm
    {
        public long? ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Percentage { get; set; }
        public bool Active { get; set; }
    }
}