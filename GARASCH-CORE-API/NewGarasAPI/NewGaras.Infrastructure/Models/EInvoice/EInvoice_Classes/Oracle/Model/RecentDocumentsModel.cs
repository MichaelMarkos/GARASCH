using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Response;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model
{
    public class RecentDocumentsModel
    {
        public List<Result> result { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Result
    {
        public string publicUrl { get; set; }
        public string uuid { get; set; }
        public string submissionUUID { get; set; }
        public string longId { get; set; }
        public string internalId { get; set; }
        public string typeName { get; set; }
        public string documentTypeNamePrimaryLang { get; set; }
        public string documentTypeNameSecondaryLang { get; set; }
        public string typeVersionName { get; set; }
        public string issuerId { get; set; }
        public string issuerName { get; set; }
        public string receiverId { get; set; }
        public string receiverName { get; set; }
        public DateTime dateTimeIssued { get; set; }
        public DateTime dateTimeReceived { get; set; }
        public double totalSales { get; set; }
        public double totalDiscount { get; set; }
        public double netAmount { get; set; }
        public double total { get; set; }
        public int maxPercision { get; set; }
        public string invoiceLineItemCodes { get; set; }
        public string cancelRequestDate { get; set; }
        public string rejectRequestDate { get; set; }
        public string cancelRequestDelayedDate { get; set; }
        public string rejectRequestDelayedDate { get; set; }
        public string declineCancelRequestDate { get; set; }
        public string declineRejectRequestDate { get; set; }
        public string documentStatusReason { get; set; }
        public string status { get; set; }
        public string createdByUserId { get; set; }
        public FreezeStatus freezeStatus { get; set; }
    }
}
