using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical.UsedInResponse
{
    public class DoctorScheduleDTOForGrouping
    {
        public long ID { get; set; }
        public long DoctorID { get; set; }  // Corresponds to 'DoctorID' (bigint)
        public string DoctorName { get; set; }
        public int Capacity { get; set; }  // Corresponds to 'Capacity' (int)
        public TimeOnly IntervalFrom { get; set; }  // Corresponds to 'IntervalFrom' (time(7))
        public TimeOnly IntervalTo { get; set; }  // Corresponds to 'IntervalTo' (time(7))
        public decimal ConsultationPrice { get; set; }  // Corresponds to 'consultationPrice' (decimal(18, 2))
        public int StatusID { get; set; }  // Corresponds to 'StatusID' (int)
        public string StatusName { get; set; }
        public string StartDate { get; set; }  // Corresponds to 'StartDate' (datetime)
        public string EndDate { get; set; }  // Corresponds to 'EndDate' (datetime, nullable)
        public long? RoomId { get; set; }  // Corresponds to 'Room' (nvarchar(150))
        public string RoomName { get; set; }
        public int PercentageTypeID { get; set; }  // Corresponds to 'percentageTypeID' (int)
        public string PercentageTypeName { get; set; }
        public decimal ExaminationPrice { get; set; }  // Corresponds to 'ExaminationPrice' (decimal(18, 2))
        public long SpecialityID { get; set; }
        public string SpecialityName { get; set; }
        public int WeekDayID { get; set; }
        public string WeekDayName { get; set; }
        public int HoldQuantity { get; set; }
        public int? BranchID { get; set; }
        public string BranchName { get; set; }
    }
}
