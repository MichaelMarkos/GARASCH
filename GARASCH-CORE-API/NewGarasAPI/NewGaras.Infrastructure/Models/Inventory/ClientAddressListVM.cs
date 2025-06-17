namespace NewGaras.Infrastructure.Models.Inventory
{
    public class ClientAddressListVM
    {
        public int ID { get; set; }
        public long ClientID { get; set; }
        public int CountryID { get; set; }
        public int GovernorateID { get; set; }
        public string Address { get; set; }
        public string GovernorateName { get; set; }
        public string CountryName { get; set; }

        public bool IsDefault { get; set; }
    }
}