using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesPersonSalesOfferListFilters
    {
        [FromHeader]
        public string OfferType { get; set; }
        [FromHeader]
        public string ProductsList { get; set; }
        [FromHeader]
        public string ReleaseFilter { get; set; }
        [FromHeader]
        public string ProductType { get; set; }
        [FromHeader]
        public string ClientName { get; set; }
        [FromHeader]
        public string ProjectName { get; set; }
        [FromHeader]
        public string OfferStatus { get; set; }
        [FromHeader]
        public string ReminderDateFilter { get; set; }
        [FromHeader]
        public DateTime? From { get; set; }
        [FromHeader]
        public DateTime? To { get; set; }
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]
        public int BranchId { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public bool? HasInvoice { get; set; }
        [FromHeader]
        public DateTime? InvoiceDate { get; set; }
        [FromHeader]
        public bool? HasProject { get; set; }
        [FromHeader]
        public DateTime? ProjectDate { get; set; }
        [FromHeader]
        public bool? HasAutoJE { get; set; }
        [FromHeader]
        public bool? HasJournalEntry { get; set; }
        [FromHeader]
        public DateTime? JournalEntryDate { get; set; }
    }
}
