namespace NewGarasAPI.Models.Admin
{
    public class ImportantDateModel
    {
        public int ID { get; set; }
        public string ReminderDate { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public bool Active { get; set; }
        public string Type { get; set; }
        public string FilePath { get; set; }
    }
}