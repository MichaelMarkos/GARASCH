namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskReplysData
    {
        public long ID { get; set; }
        public long TaskID { get; set; }
        public long RecieverUserID { get; set; }
        public bool? IsFinished { get; set; } 
        public string CommentReply { get; set; }
        public string ApprovalComment { get; set; }
        public string CreatorName { get; set; }
        public string CreatorPhoto { get; set; }
        public string TaskStatus { get; set; }
        public bool? Approval { get; set; }
        public bool Active { get; set; }
        public string CreatorAttach { get; set; }
        public string ReplyAttach { get; set; }
        public long CreatedBy { get; set; }


        public string CreatorFileName { get; set; }
        public string CreatorFileExtension { get; set; }
        public string CreatorFileContent { get; set; }



        public string ReplyFileName { get; set; }
        public string ReplyFileExtension { get; set; }
        public string ReplyFileContent { get; set; }





        public string CreationDate { get; set; }
        public long ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
    }
}