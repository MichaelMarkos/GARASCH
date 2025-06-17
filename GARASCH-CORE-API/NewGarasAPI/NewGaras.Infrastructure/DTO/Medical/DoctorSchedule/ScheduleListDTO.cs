using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.DoctorSchedule
{
    public class ScheduleListDTO
    {
        public long Id { get; set; }
        public string DateFrom { get; set; }
        public string? DateTo { get; set; }
        public string IntervalFrom { get; set; }
        public string IntervalTo { get; set; }
        public int Capacity { get; set; }
        public int WeekDayID { get; set; }
        public string DayName { get; set; }
    }
}
