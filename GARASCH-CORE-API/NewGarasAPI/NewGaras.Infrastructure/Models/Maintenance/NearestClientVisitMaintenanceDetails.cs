namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class NearestClientVisitMaintenanceDetails
    {
        public long? ClientID { get; set; }
        public string ClientName { get; set; }
        public string ClientLogo { get; set; }
        public string ClientAddress { get; set; }
        public string ClientMobile { get; set; }
        public string ClientLocation { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonMobile { get; set; }
        public string ProductSerial { get; set; }
        public string ProductBrand { get; set; }
        public string ProductFabricator { get; set; }
        public string ProductType { get; set; }
        public long? AssignedToID { get; set; }
        public long? VisitMaintenanceID { get; set; }
        public string ProductName { get; set; }
        public string ProjectName { get; set; }

        public long MaintenanceForID { get; set; }
        public string VisitDate { get; set; }
        public string AssignToUserName { get; set; }
        public decimal? ContractPrice { get; set; }
        public string CurrencyName { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Location { get; set; }
        public decimal? CurrentMileageCounter { get; set; }
        public decimal? LastMileageCounter { get; set; }
        public string PlannedDate { get; set; }
        public string WorkerUserName { get; set; }


    }
}