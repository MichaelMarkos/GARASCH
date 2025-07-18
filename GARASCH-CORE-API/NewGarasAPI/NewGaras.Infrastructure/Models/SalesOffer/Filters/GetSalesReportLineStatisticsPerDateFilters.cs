﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.Filters
{
    public class GetSalesReportLineStatisticsPerDateFilters
    {
        [FromHeader]
        public long? BranchId { get; set; }
        [FromHeader]
        public long? ClientId { get; set; }
        [FromHeader]
        public long? SalesPersonId { get; set; }
        [FromHeader]
        public long? ReportCreator { get; set; }
        [FromHeader]
        public int? Year { get; set; }
        [FromHeader]
        public string ThroughName { get; set; }
        [FromHeader]
        public bool? isReviewed { get; set; }
    }
}
