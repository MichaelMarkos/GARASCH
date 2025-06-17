using NewGarasAPI.Models.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class PurchasePOInvoiceData
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public PurchasePOInvoiceVM PurchasePOInvoiceobj { get; set; }
        public List<PurchasePOItemList> PurchasePOItemList { get; set; }
    }
}
