namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryMatrialReleaseInfoForPaging : InventoryMatrialReleaseInfo
    {
        public string RequestType { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime RequestDate { get; set; }
        public string ProjectName { get; set; }
        public List<string> ProjectNameList { get; set; }
    }
}