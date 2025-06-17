namespace NewGarasAPI.Hubs
{
    public class NotificationModel
    {
        public string CompanyName { get; set; }
        public List<long>? UserIDSList { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string SenderName { get; set; }
        public string SenderImg { get; set; }
        public string SenderImg2 { get; set; }
    }
}
