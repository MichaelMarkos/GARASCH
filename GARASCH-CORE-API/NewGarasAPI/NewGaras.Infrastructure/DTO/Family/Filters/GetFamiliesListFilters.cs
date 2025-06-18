using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family.Filters
{
    public class GetFamiliesListFilters
    {
        [FromHeader]
        public string FamilyName { get; set; }
        [FromHeader]
        public int? FamilyStatusID { get; set; }
    }
}
