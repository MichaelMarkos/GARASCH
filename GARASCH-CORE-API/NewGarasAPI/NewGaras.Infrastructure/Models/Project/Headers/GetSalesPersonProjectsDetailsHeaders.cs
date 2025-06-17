namespace NewGarasAPI.Models.Project.Headers
{
    public class GetSalesPersonProjectsDetailsHeaders
    {
        [FromHeader]
        public int Month { get; set; }

        [FromHeader]
        public int Year { get; set; }

        [FromHeader]
        public int BranchId { get; set; }

        
    }
}
