namespace NewGarasAPI.Models.Project.Headers
{
    public class GetProjectMaterialReleaseItemsDetailsHeader
    {
        [FromHeader] 
        public long ProjectId { get; set; } 

        [FromHeader]
        public long clientId { get; set; }

        [FromHeader]
        public string DateFrom { get; set; }

        [FromHeader]
        public string DateTo { get; set; }

        [FromHeader]
        public int pageNum { get; set; } = 1;

        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;

    }
}
