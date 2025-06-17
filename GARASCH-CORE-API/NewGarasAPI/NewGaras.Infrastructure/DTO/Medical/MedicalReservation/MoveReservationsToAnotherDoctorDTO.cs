using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalReservation
{
    public class MoveReservationsToAnotherDoctorDTO
    {
        public long OldDoctoreID { get; set; }
        public long NewDoctorID { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public long DoctorScheduleId { get; set; }
    }
}
