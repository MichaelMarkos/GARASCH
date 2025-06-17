namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskDetailsData
    {
        public long TaskID { get; set; }
        public long UserID { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public bool NeedApproval { get; set; }
        public string CreatorAttachement { get; set; }

        public bool Read { get; set; }
        public bool Flag { get; set; }
        public bool Star { get; set; }




        public List<GetTaskDetailsList> GetTaskDetailsList { get; set; }
    }
}
