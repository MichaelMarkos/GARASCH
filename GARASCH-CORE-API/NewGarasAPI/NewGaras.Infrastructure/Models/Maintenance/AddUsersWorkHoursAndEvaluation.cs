namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class AddUsersWorkHoursAndEvaluation
    {
        public long ID { get; set; }
        public int BranchID { get; set; }
        public bool IsDeleted { get; set; }
        public int? DepartmentID { get; set; }
        public int? JobTitleID { get; set; }
        public long UserID { get; set; }
        public decimal HourNum { get; set; }
        public decimal? Evalution { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
    }
}