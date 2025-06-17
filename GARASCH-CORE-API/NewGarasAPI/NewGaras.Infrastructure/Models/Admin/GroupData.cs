namespace NewGarasAPI.Models.Admin
{
    public class GroupData
    {
        public int ID { get; set; }
        public int GroupUserCount { get; set; }
        public int GroupRoleCount { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        public List<int> RoleIDs { get; set; }
        public bool Active { get; set; }

        public List<GroupUserList> GroupUser {  get; set; }
    }
}