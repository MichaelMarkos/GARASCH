namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceForDataByClient
    {
        public long? ID { get; set; }
        public string ProductName { get; set; }
        public string ProductBrand { get; set; }
        public string ProductType { get; set; }
        public string ProductFabricator { get; set; }
        public string ProductSerial { get; set; }
        public int? CategoryID { get; set; }
        public string CategoryName { get; set; }
        public long? InventoryItemID { get; set; }
        public string InventoryItemName { get; set; }
        public int? FabOrderID { get; set; }
        public string ContractStartDate { get; set; }
        public string ContractEndDate { get; set; }
        public string LastVisitDate { get; set; }
        public int? NumVisits { get; set; }
        public long SalesOfferID { get; set; }
        public long ProjectID { get; set; }
        public long ClientID { get; set; }
        public string Stops { get; set; }
        public string Capacity { get; set; }
    }
}