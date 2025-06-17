namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetVisitsScheduleOfMaintenanceData
    {
        public long ID { get; set; }
        public long? ManagementOfMaintenanceOrderID { get; set; }
        public string Serial { get; set; }
        public string PlannedDate { get; set; }
        public string VisitDate { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByIDEnc { get; set; }
        public string CreationDate { get; set; }
        public bool Status { get; set; }
        public long? MaintenanceForID { get; set; }
        public long? AssignedToID { get; set; }
        public string AssignedTo { get; set; }
        public decimal? ClientSatisfactionAverage { get; set; }
    }
}