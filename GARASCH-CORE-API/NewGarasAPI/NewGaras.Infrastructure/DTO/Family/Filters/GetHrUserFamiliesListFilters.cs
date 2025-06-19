using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family.Filters
{
    public class GetHrUserFamiliesListFilters
    {
        [FromHeader]
        public long? HrUserID { get; set; }
        [FromHeader]
        public long? FamilyID { get; set; }
        [FromHeader]
        public bool? IsHeadOfFamily { get; set; }
        [FromHeader]
        public bool? Active { get; set; }
    }
}
