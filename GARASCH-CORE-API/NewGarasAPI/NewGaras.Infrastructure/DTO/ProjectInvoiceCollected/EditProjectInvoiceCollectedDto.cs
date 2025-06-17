using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ProjectInvoiceCollected
{
    public class EditProjectInvoiceCollectedDto
    {
        public long ID { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public IFormFile? Attachment { get; set; }
        public int PaymentMethodID { get; set; }

    }
}
