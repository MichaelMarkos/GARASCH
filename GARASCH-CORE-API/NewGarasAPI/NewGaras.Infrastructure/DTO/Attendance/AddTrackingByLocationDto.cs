using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class AddTrackingByLocationDto
    {
        public long? Id { get; set; }
        public long UserId { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly TotalHours { get; set; }

        public TimeOnly CheckIn { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    }
}
