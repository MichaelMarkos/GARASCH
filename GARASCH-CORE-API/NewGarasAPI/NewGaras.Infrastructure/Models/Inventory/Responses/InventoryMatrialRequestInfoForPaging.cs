namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryMatrialRequestInfoForPaging : InventoryMatrialRequestInfo
    {
        public string CreationDate { get; set; }
        public string RequestDate { get; set; }
        public string RequestType { get; set; }
        public long CreatedBy { get; set; }
        public string CreatorName { get; set; }
        public string ProjectName { get; set; }
        public List<string> ProjectNameList { get; set; }
    }
}