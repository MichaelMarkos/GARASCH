namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class MaintenanceOfOffer
    {
        public long? Id { get; set; }
        public string ProductName { get; set; }
        public string ProductBrand { get; set; }
        public string Problem { get; set; }

        public string ProductSerial { get; set; }
        public bool Active { get; set; }
    }
}