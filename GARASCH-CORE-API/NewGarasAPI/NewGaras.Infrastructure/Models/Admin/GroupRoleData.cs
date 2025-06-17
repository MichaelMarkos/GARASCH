using NewGaras.Infrastructure.Entities;

namespace NewGarasAPI.Models.Admin
{
    public class GroupRoleData
    {
        public int ID { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? Active { get; set; }

        public List<ViewGroupUser> GroupUser { get; set; }

        public List<ViewGroupRole> GroupRole { get; set; }

    }
}