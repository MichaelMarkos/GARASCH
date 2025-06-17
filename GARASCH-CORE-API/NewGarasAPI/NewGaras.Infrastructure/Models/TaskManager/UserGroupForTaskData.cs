namespace NewGarasAPI.Models.TaskManager
{
    public class UserGroupForTaskData
    {
        public long ID { get; set; }
        public int UserGroupID { get; set; }
        public int TaskID { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsGroup { get; set; }
        public bool flag { get; set; }
    }
}