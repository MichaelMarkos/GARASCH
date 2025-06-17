using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model
{
    public class EInvoiceDocumentModel
    {
        public string document { get; set; }
        public string transformationStatus { get; set; }
        public ValidationResults validationResults { get; set; }
        public int maxPercision { get; set; }
        public List<InvoiceLineItemCode> invoiceLineItemCodes { get; set; }
        public string uuid { get; set; }
        public string submissionUUID { get; set; }
        public string longId { get; set; }
        public string internalId { get; set; }
        public string typeName { get; set; }
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
        public string status { get; set; }
    }

    public class Error2
    {
        public object propertyName { get; set; }
        public object propertyPath { get; set; }
        public string errorCode { get; set; }
        public string error { get; set; }
        public List<InnerError> innerError { get; set; }
    }

    public class InnerError
    {
        public string propertyName { get; set; }
        public string propertyPath { get; set; }
        public string errorCode { get; set; }
        public string error { get; set; }
        public List<InnerError> innerError { get; set; }
    }

    public class InvoiceLineItemCode
    {
        public int codeTypeId { get; set; }
        public string codeTypeNamePrimaryLang { get; set; }
        public string codeTypeNameSecondaryLang { get; set; }
        public string itemCode { get; set; }
        public string codeNamePrimaryLang { get; set; }
        public string codeNameSecondaryLang { get; set; }
    }

    public class ValidationResults
    {
        public string status { get; set; }
        public List<ValidationStep> validationSteps { get; set; }
    }

    public class ValidationStep
    {
        public string name { get; set; }
        public string status { get; set; }
        public Error2 error { get; set; }
    }
}
