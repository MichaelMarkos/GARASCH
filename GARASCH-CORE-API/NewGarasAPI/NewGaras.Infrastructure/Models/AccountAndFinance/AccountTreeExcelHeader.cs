using Microsoft.AspNetCore.Mvc;


namespace NewGarasAPI.Models.Account
{
    public class AccountTreeExcelHeader
    {
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
