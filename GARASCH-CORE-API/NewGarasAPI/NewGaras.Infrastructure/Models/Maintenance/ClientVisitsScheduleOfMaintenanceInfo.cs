namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class ClientVisitsScheduleOfMaintenanceInfo
    {
        public long MaintenanceVisitId { get; set; }
        public string ClientName { get; set; }
        public string ContanctPersonName { get; set; }
        public string ContanctPersonMobile { get; set; }
        public string ProjectLocation { get; set; }
        public string MaintenanceVisitType { get; set; }
        public long? MaintenanceForID { get; set; }
        public decimal? CurrentMileageCounter { get; set; }
    }
}