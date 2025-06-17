using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Shift
{
    public class GetBranchScheduls
    {
        public int shiftNumber {  get; set; }
        public List<BranchScheduleDto> shifts { get; set; }
    }
}
