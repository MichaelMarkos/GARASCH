using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.DoctorSchedule
{
    public class GetDoctorSchedulePerWeekDTO
    {
        public string DoctorName { get; set; }
        public long? DoctorSpecialityID { get; set; }
        public string DoctorSpecialityName { get; set; }
        public List<ScheduleDaysListDTO> schedulaDaysList { get; set; }
    }
}
