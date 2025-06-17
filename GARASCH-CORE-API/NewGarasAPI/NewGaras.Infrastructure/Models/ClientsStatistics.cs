namespace NewGaras.Infrastructure.Models
{
    public class ClientsStatistics
    {
        public string SupportedBy { get; set; } //(Fax, Phone, Facebook, Showroom - factory, Ads - اعلانات, Mail, Mobile, Yellow Pages, Whats app, Website, Visit)
        public int ClientsCount { get; set; }
        public int ClientsRFQCount { get; set; }
        public int ClientsCountLastYear { get; set; }
        public string ClientsState { get; set; } //(Up, Down)
        public int DealsCount { get; set; }
        public int DealedClientsCount { get; set; }
        public decimal TotalDealsPrice { get; set; }
        public decimal TotalDealsExtraCostPrice { get; set; }
        public decimal TotalDealsPriceLastYear { get; set; }
        public string DealsState { get; set; } //(Up, Down)
    }
}