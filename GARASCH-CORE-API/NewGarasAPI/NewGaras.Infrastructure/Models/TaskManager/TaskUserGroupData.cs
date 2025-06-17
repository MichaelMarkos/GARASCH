namespace NewGarasAPI.Models.TaskManager
{
    public class TaskUserGroupData
    {
        public long ID { get; set; }
        public long TaskID { get; set; }
        public long UserGroupID { get; set; }
        public bool IsGroup { get; set; }
        public bool? Read { get; set; }
        public bool? Flag { get; set; }
        public bool? Star { get; set; }
    }
}