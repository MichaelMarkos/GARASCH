namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class VehicleMaintenanceTypeItem
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public int VehicleRateId { get; set; }
        public string VehicleRateName { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public int VehiclePriorityLevelId { get; set; }
        public int NumberOfUses { get; set; }
        public string VehiclePriorityLevelName { get; set; }
        public int? Milage { get; set; }
        public string ExpectedDate { get; set; }
        public string LastJobOrderDate { get; set; }
        public bool IsUsed { get; set; }
        public VehicleMaintenanceTypeBOM VehicleMaintenanceTypeBOM { get; set; }
        public bool isForAllModels { get; set; }
        public List<string> VehicleMaintenanceTypeForModelsStrings { get; set; }
        public List<string> VehicleMaintenanceTypeForBrandsStrings { get; set; }
        public List<string> VehicleMaintenanceTypeServiceSheduleCategories { get; set; }
        public List<string> VehicleMaintenanceTypeServiceSheduleCategoriesIds { get; set; }
        public List<string> VehicleMaintenanceTypeForModelsIds { get; set; }
    }
}