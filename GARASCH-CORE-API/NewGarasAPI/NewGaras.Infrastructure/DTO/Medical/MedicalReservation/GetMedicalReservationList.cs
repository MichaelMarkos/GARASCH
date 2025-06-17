using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalReservation
{
    public class GetMedicalReservationList
    {
        public List<GetMedicalReservation> MedicalReservationList { get; set; }

        public PaginationHeader PaginationHeader { get; set; }
    }
}
