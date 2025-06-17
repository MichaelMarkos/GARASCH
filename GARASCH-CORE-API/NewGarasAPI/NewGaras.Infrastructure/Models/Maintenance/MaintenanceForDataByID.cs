namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceForDataByID
    {
        public long? ID { get; set; }
        public string ProductName { get; set; }
        public string ProductBrand { get; set; }
        public string ProjectLocation { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public long? AreaId { get; set; }
        public string AreaName { get; set; }
        public string Floor { get; set; }
        public string Building { get; set; }
        public string Street { get; set; }
        public string Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string ProductFabricator { get; set; }
        public string ProjectName { get; set; }
        public string CategoryName { get; set; }
        public string ClientName { get; set; }
        public string ClientPhoto { get; set; }
        public string ProductSerial { get; set; }
        public int? CategoryID { get; set; }
        public long? InventoryItemID { get; set; }
        public string InventoryItemName { get; set; }
        public int? FabOrderID { get; set; }
        public int? NumVisits { get; set; }
        public long SalesOfferID { get; set; }
        public long ProjectID { get; set; }
        public long ClientID { get; set; }
        public string CreatedBy { get; set; }
        public string GeneralNote { get; set; }
        public decimal? LastMileageCounter { get; set; }
        public string InstallationDate { get; set; }
        public string ProductionDate { get; set; }
        public string ContractNumber { get; set; }
        public string PRNumber { get; set; }
        public string Stops { get; set; }
        public string Capacity { get; set; }

        public int NumberOfCheques { get; set; } = 0;

        public List<Attachment> MaintenanceProblemAttachments { get; set; }
    }
}