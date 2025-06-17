namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountAndFinanceDashboardInfo
    {
        public decimal IncomeStatment { get; set; }
        public string CashAndBank { get; set; }
        public decimal TotalFinalOfferPrice { get; set; }
        public decimal TotalProjectExtraCost { get; set; }
        public decimal TotalFinalOfferPriceWithInternalType { get; set; }
        public decimal SalesForce { get; set; }
        public decimal SalesForceCollectedPercent { get; set; }
        public decimal Purchasing { get; set; }
        public decimal PurchasingPaidPercent { get; set; }
        public decimal Inventory {  get; set; }
    }
}