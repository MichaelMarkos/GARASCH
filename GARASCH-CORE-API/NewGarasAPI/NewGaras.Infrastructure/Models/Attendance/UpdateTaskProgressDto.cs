using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Attendance
{
    public class UpdateTaskProgressDto
    {
        public long progressId {  get; set; }

        public string progressnote { get; set; }

        public decimal progressrate { get; set; }

        public DateTime? CheckOut {  get; set; }


        public List<long> TaskRequirmentsIds { get; set; }
    }
}
