using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical.Filters
{
    public class GetDoctorScheduleListFilters
    {
        [FromHeader]
        public long? SpecialtyID { get; set; }       //team ID
        [FromHeader]
        public long? DoctorID { get; set; }          //Hruser ID

        [FromHeader]
        public int? BranchID { get; set; }          
        [FromHeader]
        public string DateFrom { get; set; }
        [FromHeader]
        public string DateTo { get; set; }
        [FromHeader]
        public string DayDate { get; set; }
        [FromHeader]
        public int currentPage { get; set; } = 1;
        [FromHeader]
        public int numberOfItemsPerPage { get; set; } = 10;
    }
}
