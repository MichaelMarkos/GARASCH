using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Attendance
{
    public class UploadAttendanceSheetFile
    {
        public IFormFile AttendanceSheet { get; set; }

        public DateTime date { get; set; } = DateTime.Now;
    }
}
