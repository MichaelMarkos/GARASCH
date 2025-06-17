namespace NewGaras.Infrastructure.Models.Inventory
{
    public class MatrialReleaseItemDDL
    {
        public long ID { get; set; }
        public long MatrialReleaseNo { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public decimal QtyReleased { get; set; }
        public string ProjectName { get; set; }
        public string ExpDate { get; set; }
        public string Serial {  get; set; }
        public string FabNo { get; set; }
    }
}