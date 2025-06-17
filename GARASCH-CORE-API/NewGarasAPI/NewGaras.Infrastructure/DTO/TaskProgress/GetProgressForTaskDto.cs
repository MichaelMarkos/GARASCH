using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskProgress
{
    public class GetProgressForTaskDto
    {
        public long WorkingHoursId { get; set; }
        public long TaskId { get; set; }
        public long? ProjectID { get; set; }
        public string Note { get; set; }
        public decimal ProgressRate { get; set; }
        public string Date { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }

        public decimal TotalHours { get; set; }

        public string CheckOutDate { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
    }
}
