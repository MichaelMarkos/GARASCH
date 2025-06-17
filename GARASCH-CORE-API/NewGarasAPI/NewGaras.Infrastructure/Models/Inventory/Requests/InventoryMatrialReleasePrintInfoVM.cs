namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class InventoryMatrialReleasePrintInfoVM
    {
        public long? Id { get; set; }
        public long InventoryMatrialReleaseOrderId { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonMobile {  get; set; }
        public string ClientAddress { get; set; }
        public string ShippingMethod { get; set; }
        public string Comment { get; set; }
        public decimal? PackagingQTY { get; set; }
        public long? ProjectId { get; set; }
    }
}