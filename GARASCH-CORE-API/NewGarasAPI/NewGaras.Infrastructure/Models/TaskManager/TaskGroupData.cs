namespace NewGarasAPI.Models.TaskManager
{
    public class TaskGroupData
    {
        public long GroupID { get; set; }
        public bool IsGroup { get; set; }
        public string GroupName { get; set; }


        public List<TaskUserData> TaskGroupUsersList { set; get; }
        public bool Flag { get; set; }

    }
}