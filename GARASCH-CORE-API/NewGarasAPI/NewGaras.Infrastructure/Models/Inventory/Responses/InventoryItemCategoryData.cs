using NewGaras.Infrastructure.Models.AccountAndFinance;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryItemCategoryData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsFinalProduct { get; set; }

        public int CategoryTypeId { get; set; }

        public string CategoryTypeName { get; set; }
        public List<InventoryItemInfo> InventoryItemList { get; set; }
    }
}