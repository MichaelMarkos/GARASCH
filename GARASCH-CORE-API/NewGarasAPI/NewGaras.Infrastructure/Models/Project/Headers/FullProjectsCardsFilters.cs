using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.Headers
{
    public class FullProjectsCardsFilters
    {
        [FromHeader]
        public string OfferType { get; set; }
        [FromHeader]
        public string ProjectsStatus { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; }
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; }
        [FromHeader]
        public long ClientId { get; set; }
        [FromHeader]
        public long ProjectId { get; set; }
        [FromHeader]
        public string ProjectOfferTypesFilters { get; set; }
        [FromHeader]
        public string MaintenanceTypesFilters { get; set; }
        [FromHeader]
        public string Location {  get; set; }
        [FromHeader]
        public long SalesPersonBranchId { get; set; }
        [FromHeader]
        public long ProjectManagerId { get; set; }
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]
        public DateTime? ProjectCreationFrom { get; set; }
        [FromHeader]
        public DateTime? ProjectCreationTo { get; set; }
        [FromHeader]
        public bool? SortByRemainCollections { get; set; } = false;
        [FromHeader]
        public bool? SortByRemainType { get; set; } = false;
    }
}
