using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalReservation
{
    public class GetMedicalReservationDTO
    {
        public long ID { get; set; }
        public long DoctorID { get; set; }
        public string DoctorName { get; set; }
        public int Serial { get; set; }
        public string ReservationDate { get; set; }
        public long PatientID { get; set; }
        public string PatientName { get; set; }
        public int PatientTypeID { get; set; }
        public string PatientTypeName { get; set; }
        public long DoctorScheduleID { get; set; }
        public long SpecialtyID { get; set; }
        public string SpecialtyName { get; set; }
        public long Room { get; set; }
        public decimal ExaminationPrice { get; set; }
        public decimal consultationPrice { get; set; }
        public string DoctorIntervalFrom { get; set; }
        public string DoctorIntervalTo { get; set; }
        public int DayID { get; set; }
        public string DayName { get; set; }
        public long? ParentID { get; set; }
        public decimal FinalAmount { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
        public long ParentSalesOfferID { get; set; }
        public long? ChildReservationID { get; set; }
        public long? ChildSalesofferID { get; set; }
        public int? paymentMethodId { get; set; }
        public string paymentMethodName { get; set; }

        public List<ReservationAddonDTO> Addons { get; set; }
    }
}
