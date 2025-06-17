namespace NewGarasAPI.Models.Admin
{
    public class AddGroupRoleData
    {
        public int ID { get; set; }
        public int[] RoleID { get; set; }
        public int GroupID { get; set; }
        public int[] UserID { get; set; }
        public string Name { get; set; }
        public string RoleName { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
    }
}
