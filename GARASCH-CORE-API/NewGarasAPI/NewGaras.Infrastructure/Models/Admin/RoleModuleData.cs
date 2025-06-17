namespace NewGarasAPI.Models.Admin
{
    public class RoleModuleData
    {
        public int ID { get; set; }
        public int RoleID { get; set; }
        public long ModuleID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string RoleName { get; set; }
        public string ModuleName { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? Active { get; set; }
    }
}