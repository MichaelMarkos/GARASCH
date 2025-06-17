namespace NewGarasAPI.Models.AccountAndFinance
{
    public class EntryAccount
    {
        public long AccountID { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public string CategoryID { get; set; } //FromDTMainType
        public string CategoryName { get; set; } //FromDTMainType
        public long? IncOrExTypeID { get; set; }
        public string IncOrExTypeName { get; set; } // FromExpIncTypeName
        public long? MethodTypeID { get; set; }
        public string MethodTypeName { get; set; }
        public string AccountTypeName { get; set; }
        public string SignOfAccount { get; set; }
        public long? ClinetID { get; set; }
        public string ClinetName { get; set; }
        public long? ProjectID { get; set; }
        public long? OfferID { get; set; }
        public string ProjectName { get; set; }
        public long? SupplierID { get; set; }
        public string SupplierName { get; set; }
        public long? PurchasePOID { get; set; }
        public string PurchasePOName { get; set; }
        public long? ClientAccountId { get; set; }
        public long? SupplierAccountId { get; set; }
        public long? AdvanciedTypeID { get; set; }
        //public string AdvanciedTypeName;
        public string Comment { get; set; }

        public bool? FromAccount { get; set; }


        // If Inserted another Currency
        public decimal? RateToAnotherCU { get; set; }
        public decimal? AmountAnotherCU { get; set; }
    }
}
