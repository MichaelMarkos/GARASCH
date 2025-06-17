

namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountMovementReportsHeader
    {
        [FromHeader]
        public string FileExtension { set; get; } = null;
        [FromHeader]
        public string AccountsList { set; get; } = "";
        [FromHeader]
        public bool OrderByCreationDate { set; get; } = false;
        [FromHeader]
        public bool CalcWithoutPrivate { set; get; } = false;
        [FromHeader]
        public string FromDate { set; get; } = new DateTime(DateTime.Now.Year, 1, 1).ToShortDateString();
        [FromHeader]
        public string ToDate { set; get; } = new DateTime(DateTime.Now.Year + 1, 1, 1).ToShortDateString();
        [FromHeader]
        public string CompanyName { set; get; }
    }
}
