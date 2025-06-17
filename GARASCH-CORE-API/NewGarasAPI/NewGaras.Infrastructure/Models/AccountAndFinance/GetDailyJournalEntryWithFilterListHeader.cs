namespace NewGarasAPI.Models.AccountAndFinance
{
    public class GetDailyJournalEntryWithFilterListHeader
    {
        [FromHeader]
        public long InventoryStoreID { get; set; } = 0;
        [FromHeader]
        public long SalesOfferID { get; set; } = 0;
        [FromHeader]
        public long ProjectID { get; set; } = 0;
        [FromHeader]
        public long InvoiceID { get; set; } = 0;
        [FromHeader]
        public long ClientID { get; set; } = 0;
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
    }
}
