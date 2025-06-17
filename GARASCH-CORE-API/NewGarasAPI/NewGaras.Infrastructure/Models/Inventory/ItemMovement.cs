namespace NewGaras.Infrastructure.Models.Inventory
{
    public class ItemMovement
    {
        public DateTime? DateFilter { get; set; }
        public string Date { get; set; }
        public string CreationDate { get; set; }
        public string StoreName { get; set; }
        public long OrderID { get; set; }
        public string OperationType { get; set; }
        public double Qty { get; set; }
        public decimal HoldQty { get; set; }
        public string HoldComment { get; set; }
        public string ReqUOM { get; set; }
        public double CumilativeQty { get; set; }
        public string FromUser { get; set; }
        public string FromSupplier { get; set; }
        public string FromDepartment { get; set; }
        public string OrderType { get; set; }

        // Extra Data For PO Item

        public long? ID { get; set; }
        public long? ParentID { get; set; }
        public long? POID { get; set; }
        public string ExpDate { get; set; }
        public string ItemSerial { get; set; }
        public decimal? RemainBalance { get; set; }
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public decimal? RateToEGP { get; set; }
        public decimal? POInvoicePriceEGP { get; set; }
        public decimal? POInvoiceUnitCostEGP { get; set; }
        public decimal? remainItemCostEGP { get; set; }
        public decimal? remainItemCostOtherCU { get; set; }


        // Extra data (Propject -Client) if released
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public long? ClientId { get; set; }
        public long? SupplierId { get; set; }
        public long? ProjectId { get; set; }
    }
}