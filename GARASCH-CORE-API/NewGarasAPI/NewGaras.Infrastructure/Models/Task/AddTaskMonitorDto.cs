namespace NewGaras.Infrastructure.Models.Task
{
    public class AddTaskMonitorDto
    {
        public long TaskID { get; set; }
        public long UserID { get; set; }
        public DateTime CreationDateTime { get; set; } = DateTime.Now;
        public IFormFile Img { get; set; }
        public string TabName { get; set; }

        public string AppName { get; set; }
    }
}