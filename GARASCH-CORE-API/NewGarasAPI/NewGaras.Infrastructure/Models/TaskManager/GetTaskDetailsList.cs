namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskDetailsList
    {
        public long TaskID { get; set; }
        public long RecieverUserID { get; set; }
        public bool IsFinished { get; set; }
        public string CommentReply { get; set; }
        public string ApprovalComment { get; set; }
        public bool Approval { get; set; }
        public bool Active { get; set; }
        public string CreatorAttach { get; set; }
        public string ReplyAttach { get; set; }
    }
}