namespace NewGarasAPI.Models.ProjectFabrication.Headers
{
    public class GetInstallatioAndFabricationOrdersCardsHeader
    {
        [FromHeader]
        public long ProjectID { get; set; }

        [FromHeader]
        public int CurrentPage { get; set; }

        [FromHeader]
        public int NumberOfItemsPerPage {  get; set; }

        [FromHeader]
        public DateTime DateFrom { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);

        [FromHeader]
        public DateTime DateTo { get; set; } = new DateTime(DateTime.Now.Year + 1,1,1);
    }
}
