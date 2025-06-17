namespace NewGarasAPI.Models.Admin
{
    public class BundleModuleData
    {
        public int ID { get; set; }
        public int? ParentID { get; set; }
        public string BundleOrModuleName { get; set; }
        public long[] BundleOrModuleIDs { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }

    }
}