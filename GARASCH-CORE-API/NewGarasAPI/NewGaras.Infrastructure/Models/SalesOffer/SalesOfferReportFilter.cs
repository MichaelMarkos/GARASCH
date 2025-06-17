using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferReportFilter
    {
        [FromHeader]
        public string OfferStatus { get; set; } = null;

        [FromHeader]

        public string ProductType { get; set; } = null;

        [FromHeader]

        public string ClientName { get; set; } = null;

        [FromHeader]

        public string ProjectName { get; set; } = null;

        [FromHeader]

        public DateTime From { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        [FromHeader]

        public DateTime To { get; set; } = DateTime.Now;
        [FromHeader]

        public long SalesPersonId { get; set; } = 0;
        [FromHeader]

        public int BranchId { get; set; } = 0;
        [FromHeader]

        public bool CalcWithoutPrivate { get; set; } = false;
        [FromHeader]

        public bool OrderByCreationDate { get; set; } = false;
    }
}
