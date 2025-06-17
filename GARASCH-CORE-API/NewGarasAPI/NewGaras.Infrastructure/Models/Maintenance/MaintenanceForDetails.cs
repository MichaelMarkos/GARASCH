using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Models.Maintenance
{
      public class MaintenanceForDetails
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string Fabricator { get; set; }
        public string ProductSerial { get; set; }
        public long ClientID { get; set; }
        public string ClientName { get; set; }
        public string ClientMobile { get; set; }
        public string ContactPersonMobile { get; set; }
        public string ContractType { get; set; }
        public string ClientAddress { get; set; }
        public string ClientLocation { get; set; }
        public string ContactPersonName { get; set; }
        public string ClientLogo { get; set; }
        public string ProjectName { get; set; }
        public string ProjectLocation { get; set; }
        public string InventoryCategory { get; set; }
        public string lastVisitDate { get; set; }
        public string NextPlannedDate { get; set; }
        public long NextVisitId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Location { get; set; }
        public int remainVisitsNo { get; set; }
        public decimal? NumberOfVisitsWithOutContract { get; set; }
        public decimal? CostVisitsWithOutContract { get; set; }
        public decimal? ManagementMaintenanctOrderID { get; set; }
        public decimal? ClientSatisfactionRate { get; set; }
        public GetContractDetails ContractDetailsList { get; set; }
        public long MaintenanceForID { get; set; }
        public long VisitMaintenanceID { get; set; }
        public long? AreaId { get; set; }
        public string AreaName { get; set; }
        public bool WithContract {  get; set; }
        public bool ClientIsBlocked { get; set; }
    }
}