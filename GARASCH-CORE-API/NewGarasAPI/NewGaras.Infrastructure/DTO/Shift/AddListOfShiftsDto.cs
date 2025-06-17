using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Shift
{
    public class AddListOfShiftsDto
    {
        public List<AddShiftDto> shiftDtos {  get; set; }

        public bool? ApplyShiftsForAll { get; set; } = null;
    }
}
