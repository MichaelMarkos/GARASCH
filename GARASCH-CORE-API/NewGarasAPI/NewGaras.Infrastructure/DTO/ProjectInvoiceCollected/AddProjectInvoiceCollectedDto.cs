using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ProjectInvoiceCollected
{
    public class AddProjectInvoiceCollectedDto
    {
        public long ProjectInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public int? PaymentMethodId { get; set; }
        public IFormFile? Attachment { get; set; }
    }
}
