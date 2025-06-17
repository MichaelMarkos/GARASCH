namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetUsersWorkHoursAndEvaluation
    {
        public long ID { get; set; }
        public long MaintenanceReportID { get; set; }
        public long BranchID { get; set; }
        public long DepartmentID { get; set; }
        public long JobTitleID { get; set; }
        public long UserID { get; set; }
        public decimal HourNum { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public decimal? Evalution { get; set; }
        public string Comment { get; set; }
        public string DepartmentName { get; set; }
        public string UserPhoto { get; set; }
        public string UserName { get; set; }
    }
}