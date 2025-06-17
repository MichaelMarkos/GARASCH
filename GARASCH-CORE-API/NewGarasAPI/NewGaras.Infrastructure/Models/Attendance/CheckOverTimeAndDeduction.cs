using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Attendance
{
    public class CheckOverTimeAndDeduction
    {
        public decimal overtimerate {  get; set; }
        public bool overtimeAllowed { get; set; }
        public decimal deductionrate { get; set; }
        public bool deductionAllowed { get; set; }
        public decimal overtimehours { get; set; }
        public decimal delayinghours { get; set; }
        public bool Error { get; set; } = false;
    }
}
