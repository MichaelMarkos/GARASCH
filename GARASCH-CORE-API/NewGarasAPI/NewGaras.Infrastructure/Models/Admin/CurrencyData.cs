namespace NewGarasAPI.Models.Admin
{
    public class CurrencyData
    {
        public int? ID { get; set; }
        public string CurrencyName { get; set; }
        public string ShortCurrencyName { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }


        public bool? IsLocal { get; set; }
    }
}
