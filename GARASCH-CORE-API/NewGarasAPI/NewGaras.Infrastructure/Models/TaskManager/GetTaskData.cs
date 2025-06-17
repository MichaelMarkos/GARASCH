namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskData
    {
        public long? ID { get; set; }

        public bool Active { get; set; }
        public bool? IsFinished { get; set; }

        public bool? NeedApproval { get; set; }
        public bool? IsTaskOwner { get; set; }
        public string Piriority { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExpireDate { get; set; }

        public string CreationDate { get; set; }
        public string CreatoreName { get; set; }
        public string TaskCategory { get; set; }
        public bool GroupReply { get; set; }
        public string TaskPage { get; set; }
        public string TaskUser { get; set; }
        public string TaskTypeName { get; set; }
        public string TaskSubject { get; set; }
        public string BranchName { get; set; }
        public string CreatorPhoto { get; set; }
        public string TaskStatus { get; set; }
        public int TaskCount { get; set; }
        public int RejectedNo { get; set; }
        public int TaskCreatorCount { get; set; }
        public int TaskTypeID { get; set; }
        public long CreatedBy { get; set; }

        public string taskAttachements { get; set; }
        public string taskAttachementExtension { get; set; }
        public string taskAttachementSize { get; set; }
        public string taskAttachementName { get; set; }
        public bool? ScreenMonitoring { get; set; }
        public bool? AllowTime { get; set; }
        public decimal? ProjectBudget { get; set; }
        public string? Currency { get; set; }
        public decimal? Weight { get; set; }
        public List<TaskUserGroupData> TaskUserGroupList { get; set; }

        public List<ProjectManagerAndAdminModel> ProjectManagers { get; set; }

        public List<ProjectManagerAndAdminModel> ProjectAdmins { get; set; }
        public decimal ApprovedProgressTotalHours { get; set; }
        public decimal PendingProgressTotalHours { get; set; }
        public decimal RejectedProgressTotalHours { get; set; }
        public TimeOnly? CheckIn {  get; set; }
        public decimal ApprovedTaskExpenses { get; set; }
        public decimal PendingTaskExpenses { get; set; }
        public decimal RejectedTaskExpenses { get; set; }
        public long? ProjectId { get; set; }
        public long? ProjectWorkFlowId { get; set; }
        public long? ProjectSprintId { get; set; }
        public bool? UnitRateService { get; set; }

    }
}
