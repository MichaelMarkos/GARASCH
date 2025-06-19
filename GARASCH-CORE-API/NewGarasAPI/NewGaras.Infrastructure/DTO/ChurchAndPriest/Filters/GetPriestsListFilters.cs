using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ChurchAndPriest.Filters
{
    public class GetPriestsListFilters
    {
        public string PriestName { get; set; }
        public long? ChurchID { get; }
        public long? EparchyID { get; set; }
    }
}
