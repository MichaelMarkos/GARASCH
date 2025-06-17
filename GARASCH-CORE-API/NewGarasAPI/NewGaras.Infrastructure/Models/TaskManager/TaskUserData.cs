namespace NewGarasAPI.Models.TaskManager
{
    public class TaskUserData
    {
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string UserPhoto { get; set; }
        public bool IsCreator { get; set; }

        public string? JobTitle { get; set; }
        public bool Flag { get; set; }
    }
}