namespace NewGarasAPI.Models.Admin
{
    public class ExpensisTypeData
    {
        public int? ID { get; set; }
        public string ExpensisTypeName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
    }
}
