using NewGaras.Infrastructure.Models.TaskMangerProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    public class SentToSupplier
    {
        
        long pOID;
        bool? sentToSupp;
        string deliveryDate;
        string sendingMethod;
        long? contactPersonID;
        List<AddAttachment> files;

        [FromForm]
        public long POID
        {
            get { return pOID; }
            set { pOID = value; }
        }
        [FromForm]
        public bool? SentToSupp
        {
            get { return sentToSupp; }
            set { sentToSupp = value; }
        }
        [FromForm]
        public string DeliveryDate
        {
            get { return deliveryDate; }
            set { deliveryDate = value; }
        }
        [FromForm]
        public string SendingMethod
        {
            get { return sendingMethod; }
            set { sendingMethod = value; }
        }
        [FromForm]
        public long? ContactPersonID
        {
            get { return contactPersonID; }
            set { contactPersonID = value; }
        }
        [FromForm]
        public List<AddAttachment> Files
        {
            get { return files; }
            set { files = value; }
        }
    }

}
