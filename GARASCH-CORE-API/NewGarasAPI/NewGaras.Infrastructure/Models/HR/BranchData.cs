namespace NewGarasAPI.Models.HR
{
    public class BranchData
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public string BranchName { get; set; }
        public string CountryName { get; set; }
        public string GovernorateName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public long CreatedById { get; set; }
        public string ModifiedBy { get; set; }
        public long? ModifiedById { get; set; }   
        public int CountryID { get; set; }
        public int GovernorateID { get; set; }
    }
}
