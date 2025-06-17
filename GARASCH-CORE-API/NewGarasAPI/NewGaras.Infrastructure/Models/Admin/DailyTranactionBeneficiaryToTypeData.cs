namespace NewGarasAPI.Models.Admin
{
    public class DailyTranactionBeneficiaryToTypeData
    {
        public int? ID { get; set; }
        public string BeneficiaryName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public long CreatedById { get; set; }
        public string ModifiedBy { get; set; }
        public long? ModifiedById { get; set; }
        public bool Active { get; set; }
    }
}
