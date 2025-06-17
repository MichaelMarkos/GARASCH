using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical.Filters
{
    public class GetDoctorScheduleListGroupByDoctorNameFilters
    {
        [FromHeader]
        public long? SpecialtyID { get; set; }       //team ID
        [FromHeader]
        public long? DoctorID { get; set; }          //Hruser ID
        [FromHeader]
        public int? BranchID { get; set; }          //Hruser ID
        [FromHeader]
        public string DayDate { get; set; }
        [FromHeader]
        public string searchKey { get; set; }
        [FromHeader]
        public bool SortByDoctornameAsce { get; set; }
        [FromHeader]
        public bool SortByDoctornameDesc { get; set; }

    }
}
