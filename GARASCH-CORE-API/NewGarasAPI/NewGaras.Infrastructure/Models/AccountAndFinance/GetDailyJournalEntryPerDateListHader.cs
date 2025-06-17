namespace NewGarasAPI.Models.AccountAndFinance
{
    public class GetDailyJournalEntryPerDateListHader
    {
        [FromHeader]
        public long InventoryStoreID { get; set; } = 0;
        [FromHeader]
        public long AccountID { get; set; } = 0;
        [FromHeader]
        public long AdvancedTypeID { get; set; } = 0;
        [FromHeader]
        public string DateToGetList { get; set; } = null;
        [FromHeader]
        public bool SortByCreationDate { get; set; } = true;
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public bool IsDeletedOrReversed { get; set; } = false;
        [FromHeader]
        public bool IsGeneral { get; set; } = false;
        [FromHeader]
        public string HasAutoJE { get; set; } = null;
        [FromHeader]
        public string ItemSerial { get; set; } = null;
        [FromHeader]
        public string SearchKey { get; set; } = null;
    }
}
