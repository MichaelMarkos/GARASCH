namespace NewGarasAPI.Models.Admin
{
    public class DashboardInfo
    {
        public decimal AccountsAndFinance {  get; set; }
        public decimal SalesForceAndClients { get; set; }
        public decimal InventoryAndStores { get; set; }
        public decimal PurchasingAndSuppliers { get; set; }
        // Project Deltails
        public long CountOFOpenProject { get; set; }
        public long CountOFClosedProject { get; set; }
        public decimal TotalCollectionActiveProjects { get; set; }
        public decimal PercentCollectionActiveProjects { get; set; }
    }
}