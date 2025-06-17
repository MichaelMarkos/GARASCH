namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MainDayVM
    {
        public string Day { get; set; }
        public string Date { get; set; }


        public List<MaintenanceValuesByDay> MaintenanceValuesByDayList { get; set; }
    }
}