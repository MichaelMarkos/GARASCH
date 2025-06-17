namespace NewGarasAPI.Models.HR
{
    public class RoleData
    {
        public int ID { get; set; }
        public int ModuleID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public long[] RoleModuleIDs { get; set; }
        public string CreatedBy { get; set; }
        public long CreatedById { get; set; }
        public string ModifiedBy { get; set; }
        public long? ModifiedById { get; set; }
        public bool Active { get; set; }
    }
}
