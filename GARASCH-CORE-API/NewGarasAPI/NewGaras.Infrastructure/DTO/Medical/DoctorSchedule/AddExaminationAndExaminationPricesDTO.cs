using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.DoctorSchedule
{
    public  class AddExaminationAndExaminationPricesDTO
    {
        public long? DoctorID { get; set; }
        //public int? DepartmentID { get; set; }
        public long? SpecialityID { get; set; }      //team ID

        public decimal ExaminationPrice { get; set; }
        public decimal consultationPrice { get; set; }
    }
}
