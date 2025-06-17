namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryAndStoresDashboardInfo
    {
        public long InventoryItemsNo { get; set; }
        public long InventoryStoreItemsPricedNo { get; set; }
        public decimal InventoryStoreItemsTotalAmount { get; set; }
        public long ExpiredItemsNo { get; set; }
        public decimal ExpiredItemsTotalAmount { get; set; }
        public long LowStockItemsNo { get; set; }
        public decimal TotalLowStockItems {  get; set; }
        public long AddingOrderNo { get; set; }
        public long AddingOrderItemsNo { get; set; }
        public long AddingOrderFromSupplierNo { get; set; }

        public long MaterialRequestOpenOrdersNo { get; set; }
        public long MaterialRequestOpenOrdersItemsNo { get; set; }
        public long MaterialRequestClosedOrdersNo { get; set; }
        public long MaterialRequestClosedOrdersItemsNo { get; set; }

        public long MaterialReleasedOrdersNo { get; set; }
        public long MaterialReleasedOrdersItemsNo { get; set; }
        public long InternalBackOrdersNo { get; set; }
        public long InternalBackOrdersItemsNo { get; set; }
        public long InternalTransferOrdersNo { get; set; }
        public long InternalTransferOrdersItemsNo { get; set; }
        public long ExternalBackOrdersNo { get; set; }
        public long ExternalBackOrdersItemsNo { get; set; }
        public long ExternalBackOrdersToSupplierNo { get; set; }
        public long FinalProductOrdersNo { get; set; }
        public long FinalProductOrdersItemsNo { get; set; }
        public long FinalProductOrdersFromProjectNo { get; set; }

        public bool HaveInventoryAdjustingReport {  get; set; }
        public int CountOFInventoryAdjustingReport { get; set; }
    }
}