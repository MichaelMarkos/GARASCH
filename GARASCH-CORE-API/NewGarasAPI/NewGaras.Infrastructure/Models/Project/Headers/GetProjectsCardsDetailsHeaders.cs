using Microsoft.AspNetCore.Mvc;

namespace NewGarasAPI.Models.Project.Headers
{
    public class GetProjectsCardsDetailsHeaders
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
        public long ProjectId { get; set; }

        [FromHeader]
        public long ClientId { get; set; }

        [FromHeader]
        public string ProjectOfferTypesFilters { get; set; }

        [FromHeader]
        public string MaintenanceTypesFilters { get; set; }

        [FromHeader]
        public string Location { get; set; }

        [FromHeader]
        public long SalesPersonBranchId { get; set; }

        [FromHeader]
        public long ProjectManagerId { get; set; }

        [FromHeader]
        public long SalesPersonId { get; set; }

        [FromHeader]
        public int BranchId { get; set; }

        [FromHeader]
        public int Month { get; set; }

        [FromHeader]
        public int Year { get; set; }

        [FromHeader]
        public bool? SortByRemainCollections { get; set; }

        [FromHeader]
        public bool SortByRemainType { get; set; }

        [FromHeader]
        public string ProjectCreationFrom { get; set; }         //string because we can send specific error and the front end deal with it

        [FromHeader]
        public string ProjectCreationTo { get; set; }           //string because we can send specific error and the front end deal with it
    }
}
