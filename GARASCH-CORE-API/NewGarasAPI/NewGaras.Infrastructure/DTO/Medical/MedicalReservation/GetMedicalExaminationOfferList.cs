using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalReservation
{
    public class GetMedicalExaminationOfferList
    {
        public long Id { get; set; }
        public long DoctorId { get; set; }
        public string offerName { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; } 
        public bool Active { get; set; }

    }
}
