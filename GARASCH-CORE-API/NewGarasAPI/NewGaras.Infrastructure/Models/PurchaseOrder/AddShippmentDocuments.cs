using NewGaras.Infrastructure.Models.PurchaseOrder.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    public class AddShippmentDocuments
    {
        List<PurchaseDocument> documents;
        long purchasePOShipmentID;

        [FromForm]
        public List<PurchaseDocument> Documents
        {
            get { return documents; }
            set { documents = value; }
        }
        [FromForm]
        public long PurchasePOShipmentID
        {
            get { return purchasePOShipmentID; }
            set { purchasePOShipmentID = value; }
        }
    }

}
