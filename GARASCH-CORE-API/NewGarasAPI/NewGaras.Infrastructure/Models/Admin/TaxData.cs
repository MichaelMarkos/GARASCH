namespace NewGarasAPI.Models.Admin
{
    public class TaxData
    {
        public int? ID { get; set; }
        public string TaxName { get; set; }
        public string TaxCode { get; set; }
        public string TaxType { get; set; }
        public decimal? TaxPercentage { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public long CreatedById { get; set; }
        public string ModifiedBy { get; set; }
        public long? ModifiedById { get; set; }
        public bool Active { get; set; }
        public bool? IsPercentage { get; set; }
    }
}