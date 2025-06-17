namespace NewGarasAPI.Models.Admin
{
    public class AddImportantDateRequest
    {
        public int ID { get; set; }
        public string ReminderDate { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string FileContent { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
    }
}
