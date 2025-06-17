namespace NewGarasAPI.Models.HR
{
    public class BeforeUpdatRecordEmployeeHours
    {
        public int CheckInHour { get; set; }
        public int CheckInMin { get; set; }
        public int CheckOutHour { get; set; }
        public int CheckOutMin { get; set; }
        public int NoMin { get; set; }
        public int DelayHours { get; set; }
        public int DeleyMin { get; set; }
        public int OverTimeHours { get; set; }
        public int OverTimeMin { get; set; }
        public int? AbsenceTypeId { get; set; }
    }
}
