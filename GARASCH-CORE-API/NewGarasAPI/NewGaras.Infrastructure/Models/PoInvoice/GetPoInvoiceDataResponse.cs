using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse;
using NewGaras.Infrastructure.Models.PurchesRequest.Responses;
using NewGarasAPI.Models.Admin;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PoInvoice
{
    public class GetPoInvoiceDataResponse
    {
        List<PurchasePaymentMethodData> purchasePaymentMethodDDL;
        List<SelectDDL> currencyDDL;
        List<SelectDDL> poInvoiceTypesDDL;
        List<SelectDDL> poInvoiceTaxIncludedTypesDDL;
        List<SelectDDL> poInvoiceExtraFeesTypesDDL;
        ViewPurchaseOrderResponse purchasePO;
        PurchasePoInvoice purchasePOInvoice;
        List<PoInvoiceTaxIncluded> poInvoiceTaxIncludedList;
        List<PoInvoiceExtraFees> poInvoiceExtraFeesList;
        PODailyJournalEntryDetails pODailyJournalEntryDetails;

        bool result;
        List<Error> errors;





        [DataMember]
        public PODailyJournalEntryDetails PODailyJournalEntryDetails
        {
            get
            {
                return pODailyJournalEntryDetails;
            }

            set
            {
                pODailyJournalEntryDetails = value;
            }
        }


        [DataMember]
        public List<PurchasePaymentMethodData> PurchasePaymentMethodDDL
        {
            get
            {
                return purchasePaymentMethodDDL;
            }

            set
            {
                purchasePaymentMethodDDL = value;
            }
        }
        [DataMember]
        public List<SelectDDL> CurrencyDDL
        {
            get
            {
                return currencyDDL;
            }

            set
            {
                currencyDDL = value;
            }
        }
        [DataMember]
        public List<SelectDDL> PoInvoiceTypesDDL
        {
            get
            {
                return poInvoiceTypesDDL;
            }

            set
            {
                poInvoiceTypesDDL = value;
            }
        }
        [DataMember]
        public List<SelectDDL> PoInvoiceTaxIncludedTypesDDL
        {
            get
            {
                return poInvoiceTaxIncludedTypesDDL;
            }

            set
            {
                poInvoiceTaxIncludedTypesDDL = value;
            }
        }
        [DataMember]
        public List<SelectDDL> PoInvoiceExtraFeesTypesDDL
        {
            get
            {
                return poInvoiceExtraFeesTypesDDL;
            }

            set
            {
                poInvoiceExtraFeesTypesDDL = value;
            }
        }
        [DataMember]
        public ViewPurchaseOrderResponse PurchasePO
        {
            get
            {
                return purchasePO;
            }

            set
            {
                purchasePO = value;
            }
        }
        [DataMember]
        public PurchasePoInvoice PurchasePOInvoice
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
        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }
    }

}
