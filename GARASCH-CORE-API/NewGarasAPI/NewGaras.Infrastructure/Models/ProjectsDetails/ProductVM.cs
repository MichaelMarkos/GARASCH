namespace NewGarasAPI.Models.ProjectsDetails
{
    public class ProductVM
    {
        [DataMember] 
        public long? SalesOfferProductID { get; set; }
        [DataMember] 
        public long ProductID { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember] 
        public long InventoryItemID { get; set; }
        [DataMember] 
        public string InventoryItemName { get; set; }
        [DataMember] 
        public long OfferID { get; set; }
        [DataMember] 
        public int InventoryItemCategoryID { get; set; }
        [DataMember] 
        public string InventoryItemCategoryName { get; set; }
        [DataMember] 
        public double Quantity { get; set; }
        [DataMember] 
        public DateTime? CreationDate { get; set; }
        [DataMember] 
        public bool Active { get; set; }
        [DataMember] 
        public int ProductGroupID { get; set; }
        [DataMember] 
        public string ProductGroupName { get; set; }
        [DataMember] 
        public decimal ItemPrice { get; set; }
        [DataMember] 
        public decimal TotalPrice { get; set; }
        [DataMember] 
        public string ConfirmReceivingComment { get; set; }
        [DataMember] 
        public double ConfirmReceivingQuantity { get; set; }
        [DataMember] 
        public long? BOMID { get; set; }
        [DataMember] 
        public string BOMUrl { get; set; }
        [DataMember] 
        public string BOMName { get; set; }
    }
}
