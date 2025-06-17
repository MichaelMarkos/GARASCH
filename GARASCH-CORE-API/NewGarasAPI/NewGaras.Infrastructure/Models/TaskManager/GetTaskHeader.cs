namespace NewGaras.Domain.Models.TaskManager
{
    public class GetTaskHeader
    {
        [FromHeader]
        public long TaskID { get; set; } = 0;
        [FromHeader]
        public long TaskTypeID { get; set; } = 0;
        [FromHeader]
        public long ToUserID { get; set; } = 0;
        [FromHeader]
        public string Status { get; set; } = null;
        [FromHeader]
        public string TaskCategory { get; set; } = null;
        [FromHeader]
        public string? PriorityFilter { get; set; } = null;
        [FromHeader]
        public string TaskName { get; set; } = null;
        [FromHeader]
        public bool? NeedApproval { get; set; } = null;
        [FromHeader]
        public bool? Read {  get; set; } = null;
        [FromHeader]
        public bool? Flag { get; set; } = null;
        [FromHeader]
        public bool? Star { get; set; } = null;
        [FromHeader]
        public bool? IsFinished { get; set; } = null;
        [FromHeader]
        public string DateFrom { get; set; }
        [FromHeader]
        public string DateTo { get; set; }
        [FromHeader]
        public string SearchKey { get; set; } = "";
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public bool Delayed { get; set; } = false;
        [FromHeader]
        public bool? NotActive { get; set; } = false;
        [FromHeader]
        public bool IsArchived { get; set; } = false;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        
    }
}
