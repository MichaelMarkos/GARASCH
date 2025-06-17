namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class InventoryMatrialReleaseUnionInternalBackOrderItems
    {
        [DataMember]
        public long? InventoryMatrialReleasetID { get; set; }

        [DataMember]
        public long? InventoryMatrialRequestID { get; set; }

        [DataMember]
        public long? InventoryBackOrderID { get; set; }

        [DataMember]
        public string InventoryItemName { get; set; }

        [DataMember]
        public string InventoryItemCode { get; set; }

        [DataMember]
        public decimal? Quantity { get; set; }

        [DataMember]
        public string UOMShortName { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public long? FabOrderNum { get; set; }

        [DataMember]
        public string clientName { get; set; }

        [DataMember]
        public string CreationDate { get; set; }

        //--------------------------------may be used------------------
        [DataMember]
        public long ProjectId { get; set; }

        [DataMember]
        public long ClientId { get; set; }
    }
}
