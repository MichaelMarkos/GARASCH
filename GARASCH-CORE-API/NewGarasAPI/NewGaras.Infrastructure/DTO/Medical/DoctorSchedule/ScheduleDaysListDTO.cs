using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.DoctorSchedule
{
    public class ScheduleDaysListDTO
    {
        public string DayDate { get; set; }
        public List<ScheduleListDTO> ScheduleList { get; set; }
    }
}
