using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical.Filters
{
    public class GetListOfSerialReservedFilters
    {
        [FromHeader]
        public long? DoctorID { get; set; }
        [FromHeader]
        public long? DoctorScheduleID { get; set; }
        [FromHeader]
        public string DayDate { get; set; }
    }
}
