using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectInvoice
{
    public class GetProjectInvoiceItemsModel
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string JobtitleName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Total { get; set; }
        public long ItemId { get; set; } = 0;

        public string Unit {  get; set; }

        public int UOMID { get; set; }
        public string CreationDate { get; set; }
    }
}
