namespace NewGarasAPI.Models.Admin
{
    public class TreeViewCountr
    {
        public string id { get; set; }
        public string title { get; set; }
        public int CountOfClient { get; set; }
        public string parentId { get; set; }
        public IList<TreeViewCountr> subs { get; set; }
    }
}