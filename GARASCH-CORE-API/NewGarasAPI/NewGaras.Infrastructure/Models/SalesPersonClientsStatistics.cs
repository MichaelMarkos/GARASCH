namespace NewGaras.Infrastructure.Models
{
    public class SalesPersonClientsStatistics
    {
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }

        public List<ClientsStatistics> OldClientsStatisticsList { get; set; }
        public List<ClientsStatistics> NewClientsStatisticsList { get; set; }
    }
}