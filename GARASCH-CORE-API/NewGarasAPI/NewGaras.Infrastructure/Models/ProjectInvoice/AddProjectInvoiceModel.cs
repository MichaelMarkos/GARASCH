using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectInvoice
{
    public class AddProjectInvoiceModel
    {
        public long? InvoiceId { get; set; }
        public long ProjectId { get; set; }
        public IFormFile Attachment { get; set; }
        public List<long> WorkingHoursIds { get; set; } = [];
        public List<long> TaskExpensisIds { get; set; } = [];
        public List<long> UnitServiceIds { get; set; } = [];
    }
}
