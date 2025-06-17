namespace NewGarasAPI.Models.Purchase
{
    public class PurchasePOItemList
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public string MatrialName { get; set; }
        public string POQTYUOP { get; set; }
        public decimal? UnitPrice { get; set; }
        public string TotalPrice { get; set; }
        public int CuID { get; set; }
        public string CuName { get; set; }
        public string For { get; set; }
        public decimal TotalOfTotalPrice { get; set; }
    }
}
