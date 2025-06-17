using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical.Filters
{
    public class GetDoctorSchedulePerWeekFilters
    {
        [FromHeader]
        public long? SpecialtyID { get; set; }
        [FromHeader]
        public long? DoctorID { get; set; }
        [FromHeader]
        public string DateFrom { get; set;}
        [FromHeader]
        public string DateTo { get; set; }
    }
}
