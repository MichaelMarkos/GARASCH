using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferStatisticsFilters
    {
        [FromHeader]
        public string OfferType { get; set; }
        [FromHeader]
        public string SupportedBy { get; set; }
        [FromHeader]
        public string ProductsListString { get; set; }
        [FromHeader]
        public string ReleaseFilterString { get; set; }
        [FromHeader]
        public string ProductType { get; set; }
        [FromHeader]
        public string ClientName { get; set; }
        [FromHeader]
        public string ProjectName { get; set; }
 
        [FromHeader]
        public string OfferStatus { get; set; } = "all";
        [FromHeader]
        public string ReminderDateFilterString { get; set; }
        [FromHeader]
        public string From { get; set; }
        [FromHeader]
        public string To { get; set; }
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]
        public int BranchId { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; }
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; }
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public int StoreId { get; set; }

    }
}
