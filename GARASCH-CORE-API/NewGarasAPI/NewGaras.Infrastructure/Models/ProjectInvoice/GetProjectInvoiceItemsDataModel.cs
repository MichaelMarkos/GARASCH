using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectInvoice
{
    public class GetProjectInvoiceItemsDataModel
    {
        public List<GetProjectInvoiceItemsModel> Items {  get; set; }
        public GetProjectInvoiceModel Invoice { get; set; }
    }
}
