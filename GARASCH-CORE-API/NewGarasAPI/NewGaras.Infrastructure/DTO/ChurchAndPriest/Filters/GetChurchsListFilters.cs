using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ChurchAndPriest.Filters
{
    public class GetChurchsListFilters
    {
        [FromHeader]
        public string ChurchName { get; set; }
        [FromHeader]
        public int? EparchyID { get; set; }
    }
}
