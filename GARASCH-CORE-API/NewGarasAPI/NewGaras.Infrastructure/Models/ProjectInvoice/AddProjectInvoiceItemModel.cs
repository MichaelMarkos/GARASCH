using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectInvoice
{
    public class AddProjectInvoiceItemModel
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public long HrUserId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Rate { get; set; }

        public long ProjectInvoiceId { get; set; }

        public decimal Total { get; set; }

        public int UOMID { get; set; }
    }
}
