using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.UsedInResponses
{
    public class PrSupplierOffer
    {
        public long? Id { get; set; }
        public long? PrId { get; set; }
        public long? PoId { get; set; }
        public long SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    }
}
