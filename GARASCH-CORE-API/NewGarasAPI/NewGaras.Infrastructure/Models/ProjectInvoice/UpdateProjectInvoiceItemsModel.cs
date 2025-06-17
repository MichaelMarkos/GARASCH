using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectInvoice
{
    public class UpdateProjectInvoiceItemsModel
    {
        public List<UpdateProjectInvoice> invoiceItemList {  get; set; } = new List<UpdateProjectInvoice>();
    }
}
