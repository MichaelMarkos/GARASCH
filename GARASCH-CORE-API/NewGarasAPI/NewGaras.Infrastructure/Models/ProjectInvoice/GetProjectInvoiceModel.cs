using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectInvoice
{
    public class GetProjectInvoiceModel
    {
        public long Id { get; set; }

        public long ProjectId { get; set; }
        public string ProjectName { get; set; }

        public string InvoiceSerial { get; set; }

        public string InvoiceDate { get; set; }

        public string InvoiceType { get; set;}

        public decimal Amount { get; set; }

        public decimal Collected { get; set; }

        public string CurrencyName { get; set; }

    }
}
