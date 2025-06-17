using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetTermsAndConditions
    {
        public long Id { get; set; }
        public int TermsCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
