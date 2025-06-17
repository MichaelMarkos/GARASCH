using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    [DataContract]
    public class ManageAddingOrderItemPORequest
    {
        long? id;
        long? pOID;
        bool? isNew;


        [DataMember]
        public long? Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        [DataMember]
        public long? POID
        {
            get
            {
                return pOID;
            }
            set
            {
                pOID = value;
            }
        }

        [DataMember]
        public bool? IsNew
        {
            get
            {
                return isNew;
            }
            set
            {
                isNew = value;
            }
        }

    }

}
