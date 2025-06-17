using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesOfferDiscount
    {
        public long? Id { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountValue { get; set; }
        public bool? DiscountApproved { get; set; }
        public bool? ClientApproveDiscount { get; set; }
        public long? DiscountApprovedBy { get; set; }
        public string DiscountApprovedByName { get; set; }
        public long? InvoicePayerClientId { get; set; }
        public string InvoicePayerClientName { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? Active { get; set; }
    }
}
