using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family.Filters
{
    public class GetFamilyCardsFilters
    {
        [FromHeader]
        public string familyName { get; set; }
        [FromHeader]
        public long? ChurchOfHeadID { get; set; }
        [FromHeader]
        public long? HeadOfTheFamilyID { get; set; }
        [FromHeader]
        public int currentPage { get; set; } = 1;
        [FromHeader]
        public int numberOfItemsPerPage { get; set; } = 10;
    }
}
