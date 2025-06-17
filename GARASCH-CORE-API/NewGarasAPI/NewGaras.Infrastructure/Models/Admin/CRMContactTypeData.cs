namespace NewGarasAPI.Models.Admin
{
    public class CRMContactTypeData
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public long CreatedByID { get; set; }
        public string CreatedBy { get; set; }
        public long? ModifiedByID { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
    }
}
