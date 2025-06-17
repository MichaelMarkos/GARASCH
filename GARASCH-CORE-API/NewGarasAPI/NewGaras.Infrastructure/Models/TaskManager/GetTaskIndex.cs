namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskIndex
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
        //public bool GroupReply { get; set; }
        //public string TaskPage { get; set; }
        //public string TaskUser { get; set; }
        //public string TaskTypeName { get; set; }
        public string TaskSubject { get; set; }
        //public string BranchName { get; set; }
        public string CreatorPhoto { get; set; }
        public string TaskStatus { get; set; }
        //public int TaskCount { get; set; }
        public int RejectedNo { get; set; }
        //public int TaskCreatorCount { get; set; }
        public int TaskTypeID { get; set; }
        public long CreatedBy { get; set; }

        //public string taskAttachements { get; set; }
        //--------------------------new added-----------------------------
        public long? ProjectID { get; set; }
        public long? ProjectSprintID { get; set; }
        public long? ProjectWorkFlowID { get; set; }
        //----------------------------------------------------------------
        public List<TaskUserGroupData> TaskUserGroupList { get; set; }

        public decimal LastPrgress { get; set; }
        public string test { get; set; }
    }
}
