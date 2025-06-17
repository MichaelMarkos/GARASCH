namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class InventoryMatrialReleaseItemsForProject
    {
       
        [DataMember] 
        public long InventoryMatrialReleasetID { get; set; }
        [DataMember] 
        public long InventoryMatrialRequestID { get; set; }
        [DataMember] 
        public long InventoryBackOrderID { get; set; }
        [DataMember] 
        public long InventoryItemID { get; set; }
        [DataMember] 
        public string InventoryItemName { get; set; }
        [DataMember] 
        public string InventoryItemCode { get; set; }
        [DataMember] 
        public decimal? RecivedQuantity { get; set; }
        [DataMember] 
        public string UOMShortName { get; set; }
        [DataMember] 
        public long? ProjectID { get; set; }
        [DataMember] 
        public string ProjectName { get; set; }
        [DataMember] 
        public long? FabricationOrderID { get; set; }
        [DataMember] 
        public int? FabricationOrderNumber { get; set; }
        [DataMember] 
        public long InventoryMatrialReleaseItemsID { get; set; }
        [DataMember] 
        public long InventoryMatrialRequestItemID { get; set; }
        [DataMember] 
        public string CreationDate { get; set; }
        [DataMember] 
        public string ClientName { get; set; }
        [DataMember] 
        public long? CustodianID { get; set; }
        [DataMember] 
        public string CustodianName { get; set; }
        [DataMember] 
        public long InventoryItemParentCategoryID { get; set; }
        [DataMember] 
        public string InventoryItemParentCategoryName { get; set; }
        [DataMember] 
        public decimal? InventoryItemPrice { get; set; }
        [DataMember] 
        public decimal? InventoryTotalPrice { get; set; }
        [DataMember] 
        public bool FromBOM { get; set; }
    }
}
