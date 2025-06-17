namespace NewGarasAPI.Models.TaskManager
{
    public class AddDeleteTaskUserGroupRequest
    {
        public List<UserGroupForTaskData> TaskUserGroupList { get; set; } = new List<UserGroupForTaskData>();
        public bool? DeleteAllButOne {  get; set; }
        public int? RemainUserGroupId { get; set; }
        public long? TaskId { get; set; }
    }
}
