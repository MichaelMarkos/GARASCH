

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class MissionDto
    {
        public int NumberOfStudent { get; set; }
        public decimal FromScore { get; set; } = 0;
        public DateTime? DateOfMission { get; set; }
        public List<MissionUser> userlist { get; set; }

    }
    public class MissionUser
    {
        public long userId { get; set; }
        public string userName { get; set; }
        public string status { get; set; }
        public string? ShowFile { get; set; }
        public string? comment { get; set; }
        public DateTime? DateOfupload { get; set; }
        public decimal? degree { get; set; }
    }
    public class MidTermDto
    {
        public int NumberOfStudent { get; set; }
        public decimal FromScore { get; set; } = 0;
        public DateTime? DateOfmidTerm { get; set; }
        public List<MidTermUser> userlist { get; set; }

    }
    public class MidTermUser
    {
        public long userId { get; set; }
        public string userName { get; set; }
        public string status { get; set; }
        public DateTime? Date { get; set; }
        public decimal? degree { get; set; }
    }

    public class AttendanceDto
    {
        public int NumberOfStudent { get; set; }
        public int NumberOfAttendanceStudent { get; set; }
        public decimal FromScore { get; set; } = 0;
        public DateTime? DateOfMission { get; set; }
        public List<AttendanceUser> userlist { get; set; }

    }
    public class AttendanceUser
    {
        public long userId { get; set; }
        public string userName { get; set; }
        public DateTime? Date { get; set; }
        public string status { get; set; }


        public string? ImagePath { get; set; }

        public string? SerialNum { get; set; }

    }
}
