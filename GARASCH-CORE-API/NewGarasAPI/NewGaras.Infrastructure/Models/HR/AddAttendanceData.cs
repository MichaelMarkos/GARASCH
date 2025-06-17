namespace NewGarasAPI.Models.HR
{
    public class AddAttendanceData
    {
        public long? ID { get; set; }
        public long EmployeeId { get; set; }
        public long EmployeeName { get; set; }
        public int DepartmentId { get; set; }
        public long? TeamId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string AttendanceDateSTR { get; set; }
        public int? CheckInHour { get; set; }
        public int? CheckInMin { get; set; }
        public int? CheckOutHour { get; set; }
        public int? CheckOutMin { get; set; }
        public int? NoMin { get; set; }
        public int? AbsenceTypeId { get; set; }
    }
}
