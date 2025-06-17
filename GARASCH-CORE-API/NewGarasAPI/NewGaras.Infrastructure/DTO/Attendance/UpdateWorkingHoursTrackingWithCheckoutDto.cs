using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class UpdateWorkingHoursTrackingWithCheckoutDto
    {
        public long WorkingTrackingId { get; set; }

        public DateTime Checkout {  get; set; }

        public bool ProgressModifedByAdmin { get; set; } = false;
    }
}
