namespace NewGarasAPI.Models.Admin
{
    public class EditGroupData
    {
        public int? ID { get; set; }
        public long? GroupID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }

        public List<long> UserID { get; set; }
        public List<int> RoleID { get; set; }

    }
}
