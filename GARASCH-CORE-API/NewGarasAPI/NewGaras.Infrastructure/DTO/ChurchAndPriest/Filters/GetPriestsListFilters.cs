using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ChurchAndPriest.Filters
{
    public class GetPriestsListFilters
    {
        [FromHeader]
        public string PriestName { get; set; }
        [FromHeader]
        public long? ChurchID { get; set; }
        [FromHeader]
        public long? EparchyID { get; set; }
        [FromHeader]
        public int currentPage { get; set; } = 1;
        [FromHeader]
        public int numberOfItemsPerPage { get; set; } = 10;
    }
}
