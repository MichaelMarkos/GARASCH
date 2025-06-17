namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class VisitsScheduleOfMaintenanceData
    {
        public long ID { get; set; }
        public long ManagementOfMaintenanceOrderID { get; set; }
        public string Serial { get; set; }
        public string PlannedDate { get; set; }
        public string VisitDate { get; set; }
        public bool Status { get; set; }
        public long? MaintenanceForID { get; set; }
        public long? AssignedToID { get; set; }
        public decimal? MileageCounter { get; set; }


        // Extra Data  
        public decimal? ChceckInlongitude { get; set; }
        public decimal? ChecckInlatitude { get; set; }
        public DateTime? CheckInTime { get; set; }
        public decimal? ChceckOutlongitude { get; set; }
        public decimal? CheckOutlatitude { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public DateTime? ReminderDate { get; set; }
        public string ReminderHint { get; set; }
    }
}