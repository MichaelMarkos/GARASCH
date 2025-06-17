namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceProduct
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public long ClientId { get; set; }
        public string ClientName { get; set; }
    }
}