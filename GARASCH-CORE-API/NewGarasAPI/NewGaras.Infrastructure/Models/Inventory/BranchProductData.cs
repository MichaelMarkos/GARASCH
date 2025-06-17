namespace NewGaras.Infrastructure.Models.Inventory
{
    public class BranchProductData
    {
        public int ID { get; set; }
        public int BranchID { get; set; }
        public int ProductID { get; set; }
        public string BranchName { get; set; }
        public string ProductName { get; set; }
        public bool Active { get; set; }
    }
}