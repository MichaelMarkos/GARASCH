using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Shift
{
    public class AddShiftDto
    {
        public long? Id { get; set; }

        public TimeOnly? From { get; set; }

        public TimeOnly? To { get; set; }

        public int ShiftNumber { get; set; }

        public int? WeekDayId { get; set; }

        public int? BranchId { get; set; }

        public bool? Active { get; set; } = true;
    }
}
