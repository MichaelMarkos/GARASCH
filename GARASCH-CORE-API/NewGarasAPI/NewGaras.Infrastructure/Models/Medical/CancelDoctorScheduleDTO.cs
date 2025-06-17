using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical
{
    public class CancelDoctorScheduleDTO
    {
        public long DoctorScheduleID { get; set; }
        public long DoctorID { get; set; }
        public string IntervalFrom { get; set; }
        public string IntervalTo { get; set; }
    }
}
