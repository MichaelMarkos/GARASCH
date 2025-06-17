
namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryInternalBAckOrderByDate
    {
        public string DateMonth { get; set; }
        public List<InventoryInternalBackOrderInfo> InventoryInternalBackOrderInfoList { get; set; }
    }
}