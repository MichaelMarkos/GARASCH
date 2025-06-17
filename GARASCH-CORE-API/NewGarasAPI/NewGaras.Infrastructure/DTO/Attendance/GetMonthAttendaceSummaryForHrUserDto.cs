using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetMonthAttendaceSummaryForHrUserDto
    {
        public long HrUserId { get; set; }
        //-------------Date-----------------------
        public string DayDate { get; set; }
        //-------------check Time-----------------
        public string From { get; set; }
        public string To { get; set; }

        //--------------working Hours-------------
        public string TotalHours { get; set; }
        public string HolidayHours { get; set; }
        public string VacationHours { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public string OverTime { get; set; }
        public string Delay { get; set; }

        //-------------Day Type-------------------
        public int? DayTypeID { get; set; }
        public string DayTypeName { get; set; }
        public long? AbsenceDayApprovedById { get; set; }
        public string AbsenceDayApprovedByName { get; set; }

        public string CheckOutDate { get; set; }

    }
}
