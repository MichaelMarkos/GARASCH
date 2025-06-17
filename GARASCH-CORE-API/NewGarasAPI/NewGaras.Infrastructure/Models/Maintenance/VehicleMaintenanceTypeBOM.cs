namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class VehicleMaintenanceTypeBOM
    {
        public long? BOMID { get; set; }
        public string BOMName { get; set; }
        public decimal TotalCostType1 { get; set; }
        public decimal TotalCostType2 { get; set; }
        public decimal TotalCostType3 { get; set; }
        public List<string> BOMPartitionItemsNames { get; set; }
    }
}