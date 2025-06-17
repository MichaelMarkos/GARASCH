namespace NewGarasAPI.Models.Admin
{
    public class IncomeTypeData
    {
        public long? ID { get; set; }
        public string IncomeTypeName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
    }
}
