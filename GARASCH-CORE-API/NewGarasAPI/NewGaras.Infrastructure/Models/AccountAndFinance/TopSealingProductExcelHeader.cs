namespace NewGarasAPI.Models.AccountAndFinance
{
    public class TopSealingProductExcelHeader
    {
        [FromHeader]
        public int Month { get; set; } = 0;
        [FromHeader]
        public int Year { get; set; } = 0;
        [FromHeader]
        public long SalesPersonId { get; set; } = 0;
        [FromHeader]
        public int BranchId { get; set; } = 0;
    }
}
