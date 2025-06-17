namespace NewGarasAPI.Models.Project.Headers
{
    public class InventoryMatrialReleaseUnionInternalBackOrderItemsheaders
    {
        [FromHeader]
        public long ProjectID { get; set; }

        [FromHeader]
        public long clientId { get; set; }

        [FromHeader]
        public string CreateFrom { get; set; }

        [FromHeader]
        public string createTo { get; set; }

        [FromHeader]
        public int CurrentPage { get; set; }

        [FromHeader]
        public int NumberOfItemsPerPage { get; set; }

    }
}
