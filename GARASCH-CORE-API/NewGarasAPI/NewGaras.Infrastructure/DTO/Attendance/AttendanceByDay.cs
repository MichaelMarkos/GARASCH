namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class AttendanceByDay
    {
        public long? Id { get; set; }
        public long HrUserId { get; set; }
        public string Name { get; set; }

        public string DepartmentName { get; set; }
        public string TeamName { get; set; }
        public string JobtitleName { get; set; }

        public string Date {  get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string TotalHours { get; set; }

        public string OverTimeHours { get; set; }
        public string DelayHours { get; set; }
        public string DayType { get; set; }
        public int? DayTypeId { get; set; }
        public string HrUserImg { get; set; }

        public decimal HoursNum { get; set; }

        public string CheckOutDate { get; set; }
        public string HolidayHours { get; set; }
        public string VacationHours { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
    }
}