using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical.Filters
{
    public class GetGetMedicalReservationFilters
    {
        [FromHeader]
        public long? DoctorID { get; set; }
        [FromHeader]
        public int? Serial { get; set; }
        [FromHeader]
        public string StartDate { get; set; }
        [FromHeader]
        public string EndDate { get; set; }
        [FromHeader]
        public long? PatientID { get; set; }
        [FromHeader]
        public int? PatientTypeID { get; set; }
        [FromHeader]
        //public long DoctorScheduleID { get; set; }
        public long? SpecialtyID { get; set; }
        [FromHeader]
        public long? Room { get; set; }
        [FromHeader]
        public long? ParentID { get; set; }
        [FromHeader]
        public string ReservationTyps { get; set; }
        [FromHeader]
        public int currentPage { get; set; } = 1; 
        [FromHeader]
        public int numberOfItemsPerPage { get; set; } = 10;
    }
}
