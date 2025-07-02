namespace NewGarasAPI.Models.Account
{
    public class TreeViewAccount
    {
        public string id { get; set; }
        public string title { get; set; }
        public string parentId { get; set; }
        public bool? HasChild { get; set; }
        public string Code { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Accumulative { get; set; }
        public string Currency { get; set; }
        public bool Active { get; set; }
        public long? AdvanciedTypeId { get; set; }
        public string AdvanciedTypeName { get; set; }
        public int? DataLevel { get; set; }
        public IList<TreeViewAccount> subs { get; set; }

        // for test 
        public List<AccumulativePerMonth> AccumulativePerMonth { get; set; }

        public bool HasDepreciation { get; set; }
    }
}
