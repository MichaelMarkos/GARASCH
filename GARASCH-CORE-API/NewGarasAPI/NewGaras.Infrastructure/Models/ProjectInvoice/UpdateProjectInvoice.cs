using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectInvoice
{
    public class UpdateProjectInvoice
    {
        public long ProjectInvoiceItemId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Rate { get; set; }

        public int UOMID { get; set; }
    }
}
