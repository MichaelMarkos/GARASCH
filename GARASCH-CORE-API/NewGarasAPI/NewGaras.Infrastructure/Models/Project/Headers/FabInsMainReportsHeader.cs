namespace NewGarasAPI.Models.Project.Headers
{
    public class FabInsMainReportsHeader
    {
        [FromHeader]
        public long ProjectId { get; set; }

        [FromHeader]
        public long ClientId { get; set; }

        [FromHeader]
        public string DateFrom { get; set; }

        [FromHeader]
        public string DateTo { get; set; }

        [FromHeader]
        public int CurrentPage { get; set; }

        [FromHeader]
        public int NumberOfItemsPerPage { get; set; }

    }
}
