using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.DoctorSchedule
{
    public class EditDoctorScheduleDTO
    {
        public long Id { get; set; }
        //public long? DoctorID { get; set; }  // Corresponds to 'DoctorID' (bigint)
        public int? Capacity { get; set; }  // Corresponds to 'Capacity' (int)
        public TimeOnly? IntervalFrom { get; set; }  // Corresponds to 'IntervalFrom' (time(7))
        public TimeOnly? IntervalTo { get; set; }  // Corresponds to 'IntervalTo' (time(7))
        public decimal? ConsultationPrice { get; set; }  // Corresponds to 'consultationPrice' (decimal(18, 2))
        public int? StatusID { get; set; }  // Corresponds to 'StatusID' (int)
        //public DateTime? StartDate { get; set; }  // Corresponds to 'StartDate' (datetime)
        public DateTime? EndDate { get; set; }  // Corresponds to 'EndDate' (datetime, nullable)
        public long? RoomId { get; set; }  // Corresponds to 'Room' (nvarchar(150))
        public int? PercentageTypeID { get; set; }  // Corresponds to 'percentageTypeID' (int)
        public decimal? ExaminationPrice { get; set; }  // Corresponds to 'ExaminationPrice' (decimal(18, 2))
        //public long? SpecialityId { get; set; }
        public int? WeekDayID { get; set; }
        public int? HoldQuantity { get; set; }
    }
}
