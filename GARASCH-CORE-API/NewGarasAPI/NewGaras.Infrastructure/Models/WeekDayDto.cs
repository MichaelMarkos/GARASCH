using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class WeekDayDto
    {
        public int Id { get; set; }
        public string Day { get; set; }
        public bool? IsWeekEnd { get; set; }
        public int? BranchId { get; set; }
    }
}
