using NewGaras.Infrastructure.Models.Medical.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalReservation
{
    public class AddMedicalReservationDTO
    {
        public long DoctorID { get; set; }
        public int Serial { get; set; }
        public string ReservationDate { get; set; }
        public long PatientID { get; set; }
        public int PatientTypeID { get; set; }
        public long DoctorScheduleID { get; set; }
        //public long TeamID { get; set; }
        
        public string Type { get; set; }
        public long? ParentID { get; set; }
        public decimal FinalAmount { get; set; }
        public bool Active { get; set; }
        public int? CardNumber { get; set; }
        public int PaymentMethodId { get; set; }

        public List<InventoryItemAndCategoryListDTO> InventoryItemAndCategoryList { get; set; }
    }
}
