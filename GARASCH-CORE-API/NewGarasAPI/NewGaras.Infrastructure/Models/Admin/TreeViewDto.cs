namespace NewGarasAPI.Models.Admin
{
    public class TreeViewDto
    {
        public string id { get; set; }
        public string title { get; set; }
        public string parentId { get; set; }
        public bool? HasChild { get; set; }
        public IList<TreeViewDto> subs { get; set; }
    }
}