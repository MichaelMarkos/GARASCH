using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class GetInvoicesResponse
    {
        public bool Result { get; set; }
        public List<InvoiceDataModel> InvoicesList { get; set; }
        public PaginationHeader PaginationHeader {  get; set; } 
        public List<Error> Errors { get; set; }
    }
}
