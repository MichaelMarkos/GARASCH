namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesTargetProduct
    {
        public long ID { get; set; }
        public int TargetID { get; set; }
        public int BranchID { get; set; }
        public int ProductID { get; set; }

        public float Percentage { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }

        public bool Active { get; set; }
    }
}