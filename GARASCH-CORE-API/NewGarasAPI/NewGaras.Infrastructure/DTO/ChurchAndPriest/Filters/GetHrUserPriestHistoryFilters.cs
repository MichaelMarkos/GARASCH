using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ChurchAndPriest.Filters
{
    public class GetHrUserPriestHistoryFilters
    {
        [FromHeader]
        public long HrUserId { get; set; }
        [FromHeader]
        public long? PriestID { get; set; }
    }
}
