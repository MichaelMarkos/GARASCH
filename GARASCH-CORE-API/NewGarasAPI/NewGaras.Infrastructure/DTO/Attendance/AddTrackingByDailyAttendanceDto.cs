using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class AddTrackingByDailyAttendanceDto
    {
        public long? Id { get; set; }
        public long UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalHours { get; set; }
        public string ProgressNote { get; set; }
        public decimal? progressPErcent { get; set; }

        public TimeOnly? CheckIn { get; set; }
        public TimeOnly? checkOut { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
