namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceValues
    {
        public int? PlannedVisitCount { get; set; }
        public int? Closed { get; set; }
        public int? Open { get; set; }
        public int? UsersCount { get; set; }
        public int? MonthID { get; set; }
        public decimal? WorkingHoursAverage { get; set; }
        public decimal? EvaluationAverage { get; set; }
        public string MonthName { get; set; }

    }
}