using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class AccountSalesOfferReportFilter
    {


        [FromHeader]

        public long ClientId { get; set; } = 0;

        [FromHeader]
        public DateTime From { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        [FromHeader]

        public DateTime To { get; set; } = DateTime.Now;
        [FromHeader]

        public long SupplierId { get; set; } = 0;
        [FromHeader]

        public int BranchId { get; set; } = 0;
        [FromHeader]

        public bool CalcWithoutPrivate { get; set; } = false;
        [FromHeader]

        public bool OrderByCreationDate { get; set; } = false;
        [FromHeader]
        public long? AdvancedTypeId { get; set; }
        [FromHeader]
        public long? AccountCategoryId { get; set; }
        [FromHeader]
        public string AccountIds { get; set; }
    }
}
