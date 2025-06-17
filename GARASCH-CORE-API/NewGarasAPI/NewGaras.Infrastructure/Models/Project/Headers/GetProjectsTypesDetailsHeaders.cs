namespace NewGarasAPI.Models.Project.Headers
{
    public class GetProjectsTypesDetailsHeaders
    {
        [FromHeader]
        public string ProjectsStatus { get; set; }

        [FromHeader]
        public long SalesPersonId { get; set; }

        [FromHeader]
        public int BranchId { get; set; }

        [FromHeader]
        public int Month { get; set; }

        [FromHeader]
        public int Year { get; set; }

        [FromHeader]
        public int Day { get; set; }
    }
}
