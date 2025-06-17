namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    public class ActivePODDL
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public decimal? ReqQuantity { get; set; }
        public decimal? RecivedQuantity { get; set; }
        public decimal? ExchangeFactor { get; set; }
        public string RequstionUOMShortName { get; set; }
        public string PurchasingUOMShortName { get; set; }
        public string POCreationDateDate { get; set; }
        public long SupplierID { get; set; }
        public string SupplierName { get; set; }
    }
}