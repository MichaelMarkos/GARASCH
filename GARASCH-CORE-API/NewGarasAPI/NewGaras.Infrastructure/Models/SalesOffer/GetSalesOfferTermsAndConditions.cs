using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesOfferTermsAndConditions
    {
        public long? Id { get; set; }
        public long SalesOfferId { get; set; }
        public string TermName { get; set; }
        public string TermCategoryName { get; set; }
        public string TermDescription { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? Active { get; set; }
    }
}
