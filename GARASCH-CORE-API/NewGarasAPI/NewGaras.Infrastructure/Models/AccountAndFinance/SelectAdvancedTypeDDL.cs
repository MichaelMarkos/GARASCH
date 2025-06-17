namespace NewGarasAPI.Models.AccountAndFinance
{
    public class SelectAdvancedTypeDDL
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public long? AccountCategoryId { get; set; }
        public string AccountCategoryName { get; set; }
    }
}