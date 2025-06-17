using NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PoInvoice
{
    [DataContract]
    public class AddDirectPoInvoiceRequest
    {
        PurchasePoInvoiceDirect purchasePOInvoice;
        List<DirectPurchaseOrderItem> purchasePOItemList;
        List<PoInvoiceTaxIncluded> poInvoiceTaxIncludedList;
        List<PoInvoiceExtraFees> poInvoiceExtraFeesList;


        [DataMember]
        public PurchasePoInvoiceDirect PurchasePOInvoice
        {
            get
            {
                return purchasePOInvoice;
            }

            set
            {
                purchasePOInvoice = value;
            }
        }

        [DataMember]
        public List<DirectPurchaseOrderItem> PurchasePOItemList
        {
            get
            {
                return purchasePOItemList;
            }

            set
            {
                purchasePOItemList = value;
            }
        }

        [DataMember]
        public List<PoInvoiceTaxIncluded> PoInvoiceTaxIncludedList
        {
            get
            {
                return poInvoiceTaxIncludedList;
            }

            set
            {
                poInvoiceTaxIncludedList = value;
            }
        }

        [DataMember]
        public List<PoInvoiceExtraFees> PoInvoiceExtraFeesList
        {
            get
            {
                return poInvoiceExtraFeesList;
            }

            set
            {
                poInvoiceExtraFeesList = value;
            }
        }
    }
}
