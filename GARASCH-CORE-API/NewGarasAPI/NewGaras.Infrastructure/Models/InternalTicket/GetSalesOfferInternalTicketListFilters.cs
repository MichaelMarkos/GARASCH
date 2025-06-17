using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InternalTicket
{
     public class GetSalesOfferInternalTicketListFilters
    {
        [FromHeader]
        public string OfferType { get; set; }
        [FromHeader]
        public string ClientName { get; set; } = "";
        [FromHeader]
        public string OfferStatus { get; set; } = "";
        [FromHeader]
        public DateTime? From { get; set; }
        [FromHeader]
        public DateTime? To { get; set; }
        [FromHeader]
        public long SalesPersonId { get; set; } = 0;
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public string SearchKey { get; set; } = "";
        [FromHeader]
        public long CreatorUserId { get; set; }
        [FromHeader]
        public long DepartmentId { get; set; }
        [FromHeader]
        public long CategoryId { get; set; }
        [FromHeader]
        public string SalesOfferClassifiction { get; set; }
    }
}
