using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class UpdatePosClosingDay
    {
        public long Id { get; set; }

        public string Notes { get; set; }

        public decimal ClosingDayAmount { get; set; }
    }
}
