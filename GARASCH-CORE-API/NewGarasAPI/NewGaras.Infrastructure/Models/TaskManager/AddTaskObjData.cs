namespace NewGarasAPI.Models.TaskManager
{
    public class AddTaskObjData
    {
        public int ID { get; set; }
        public int TaskFlagID { get; set; }
        public int TaskID { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExpireDate { get; set; }
        public string TaskUser { get; set; }
        public int TaskTypeID { get; set; }
        public string RefrenceNumber { get; set; }
        public string TaskCategory { get; set; }
        public string TaskSubject { get; set; }

        public int BranchID { get; set; }
        public string TaskPage { get; set; }
        public bool Active { get; set; } = true;
        public bool Flag { get; set; }
        public bool NeedApproval { get; set; }
        public string StoreName { get; set; }
        public string Location { get; set; }
        public string Tel { get; set; }
        public bool GroupReply { get; set; }
        public bool? ScreenMonitoring { get; set; }
        public bool? AllowTime { get; set; }
        public decimal? ProjectBudget { get; set; }
        public string? Currency { get; set; }
        public decimal? Weight { get; set; }
        public long? ProjectID { get; set; }
        public long? ProjectSprintId { get; set; }
        public long? ProjectWorkflowId { get; set; }
        public Attachment taskAttachements { get; set; }
        public List<AddGroupIDsData> AddGroupIDsData { get; set; }
        public List<AddUserIDData> AddUserIDData { get; set; }

        public bool DeleteAttachment { get; set; } = false;
    }
}
