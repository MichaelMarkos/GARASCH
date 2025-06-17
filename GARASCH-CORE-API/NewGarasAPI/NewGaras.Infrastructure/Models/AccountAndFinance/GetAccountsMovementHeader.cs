namespace NewGarasAPI.Models.AccountAndFinance
{
    public class GetAccountsMovementHeader
    {
        [FromHeader]
        public string AccountID { set; get; } = "1";
        [FromHeader]
        public long ClientId { set; get; } = 0;
        [FromHeader]
        public long SupplierId { set; get; } = 0;
        [FromHeader]
        public bool CalcWithoutPrivate { set; get; } = false;
        [FromHeader]
        public bool OrderByCreationDate { set; get; } = false;
        [FromHeader]
        public string FromDate { set; get; } = null;
        [FromHeader]
        public string DateTo { set; get; } = null;

    }
}
