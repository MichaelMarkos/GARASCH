namespace NewGarasAPI.Models.Purchase
{
    public class PurchasePOInvoiceVM
    {
        public long ID { get; set; }
        public long POID { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceCollectionDueDate { get; set; }
        public long? InvoiceAttachementID { get; set; }
        public string TotalInvoicePrice { get; set; }
        public string SupplierName { get; set; }
        public string POStatus { get; set; }
        public string InvoiceStatus { get; set; }
        public long PurchasePOInvoiceTypeID { get; set; }
        public string PurchasePOInvoiceTypeIDName { get; set; }
        public bool IsClosed { get; set; }
        public bool Active { get; set; }
        public long ToSupplierID { get; set; }
    }
}
