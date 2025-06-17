namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceAreaVM
    {
        public string Area { get; set; }


        public List<MaintenanceValuesByDay> MaintenanceValuesByAreaList;
    }
}