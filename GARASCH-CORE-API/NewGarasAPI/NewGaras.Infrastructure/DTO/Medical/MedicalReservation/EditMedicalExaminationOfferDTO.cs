using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalReservation
{
    public class EditMedicalExaminationOfferDTO
    {
        public long ID { get; set; }
        public long? DoctorID { get; set; }
        public string OfferName { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? Active { get; set; }

    }
}
