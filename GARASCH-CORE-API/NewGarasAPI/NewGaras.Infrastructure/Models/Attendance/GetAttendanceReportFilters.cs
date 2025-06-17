namespace NewGaras.Infrastructure.Models.Attendance
{
    public class GetAttendanceReportFilters
    {
        [FromHeader]
        public int? BranchId { get; set; }
        [FromHeader]
        public long? DepartmentId { get; set; }
        [FromHeader]
        public DateOnly? From { get; set; }
        [FromHeader]
        public DateOnly? To { get; set; }
    }
}
