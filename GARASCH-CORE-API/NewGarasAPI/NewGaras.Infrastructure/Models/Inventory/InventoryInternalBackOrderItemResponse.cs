namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryInternalBackOrderItemResponse
    {
        public List<InventoryInternalBAckOrderByDate> InventoryInternalBackOrderByDateList {  get; set; }
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}