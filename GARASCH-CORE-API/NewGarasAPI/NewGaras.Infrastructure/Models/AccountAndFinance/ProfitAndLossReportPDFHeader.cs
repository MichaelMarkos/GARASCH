namespace NewGarasAPI.Models.AccountAndFinance
{
    public class ProfitAndLossReportPDFHeader
    {
        [FromHeader]
        public bool CalcWithoutPrivate { set; get; } = false;
        [FromHeader]
        public string FromDate { set; get; } //= new DateTime(DateTime.Now.Year, 1, 1).ToShortDateString();
        [FromHeader]
        public string ToDate { set; get; } //= new DateTime(DateTime.Now.Year + 1, 1, 1).ToShortDateString();
        [FromHeader]
        public string CompanyName { set; get; }

        [FromHeader]
        public long? UserID { set; get; }
    }
}
