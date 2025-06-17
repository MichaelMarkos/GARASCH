namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskReplyHeader
    {
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public long TaskID { get; set; } = 0;
        [FromHeader]
        public string CommentSearch { get; set; } = null;
    }
}
