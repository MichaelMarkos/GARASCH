namespace NewGarasAPI.Models.Project.Headers
{
    public class GetProjectsStatisticsHeaders
    {
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
        public long SalesPersonId { get; set; }

        [FromHeader]
        public string ProjectCreationFrom { get; set; }        //string because we can send specific error and the front end deal with it

        [FromHeader]
        public string ProjectCreationTo { get; set; }          //string because we can send specific error and the front end deal with it

        [FromHeader]
        public int Year { get; set; }

        [FromHeader]
        public int Month { get; set; }
    }
}
