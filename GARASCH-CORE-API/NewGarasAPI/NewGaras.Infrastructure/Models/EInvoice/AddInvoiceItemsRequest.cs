using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class AddInvoiceItemsRequest
    {
        public bool? OldInvoice {  get; set; }
        public int? InvoiceType { get; set; }
        public string InvoiceFor {  get; set; }
        public string CreationType { get; set; }
        public long? SalesOfferID { get; set; }
        public List<InvoiceItemModel> InvoiceItemList { get; set; }
    }
}
