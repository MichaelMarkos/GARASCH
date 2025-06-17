namespace NewGarasAPI.Models.Admin
{
    public class GovernorateData
    {
        public string ID { get; set; }
        public string CountryID { get; set; }
        public string CountryRequestedID { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
    }
}
