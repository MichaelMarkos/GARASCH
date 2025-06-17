using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferExcelfilters
    {
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public string OfferStatus { get; set; }
        [FromHeader]
        public string ProductType { get; set; }
        [FromHeader]
        public string ClientName { get; set; }
        [FromHeader]
        public string ProjectName { get; set; }
        [FromHeader]
        public DateTime? FromDate { get; set; }
        [FromHeader]
        public DateTime? ToDate { get; set; }
        public bool DateFilter => FromDate.HasValue && ToDate.HasValue;
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]
        public int BranchId { get; set; }
    }
}
