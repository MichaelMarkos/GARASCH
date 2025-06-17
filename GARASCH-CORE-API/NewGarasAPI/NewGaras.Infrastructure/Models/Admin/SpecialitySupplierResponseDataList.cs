namespace NewGarasAPI.Models.Admin
{
    public class SpecialitySupplierResponseDataList
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public long CreatedById { get; set; }
        public string ModifiedBy { get; set; }
        public long? ModifiedById { get; set; }
    }
}