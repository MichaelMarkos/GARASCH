using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferForReport
    {
        public long Id { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string SalesPersonName { get; set; }
        public string OfferSerial { get; set; }
        public decimal? PercentReleasedQty { get; set; }
        public decimal? PercentReleasedValue { get; set; }
        public decimal? FinalOfferPrice { get; set; }
        public string ClientApprovalDate { get; set; }
        public string OfferType { get; set; }
        public string CreationDate { get; set; }
        public decimal GrossProfitPercentage { get; set; }
        public decimal GrossProfitValue { get; set; }
        public long HasJournalEntryId { get; set; }
        public decimal? OfferAmount { get; set; }
        public decimal? TaxValue { get; set; }
        public decimal? Discount { get; set; }
        public decimal? ExtraCost { get; set; }
        public decimal? DiscountOrExtraCostPerSalesPerson { get; set; }
        public string OfferStatus { get; set; }

        public decimal Totalcost { get; set; }

        public decimal CollectedPayment {  get; set; }
        public decimal Remain {  get; set; }

        public decimal ProductsCost { get; set; }
    }
}
