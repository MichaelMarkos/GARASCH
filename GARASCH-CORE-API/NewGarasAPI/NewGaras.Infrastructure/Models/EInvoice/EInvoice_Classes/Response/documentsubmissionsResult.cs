using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Response
{
    public class documentsubmissionsResult
    {
        public string submissionUUID { get; set; }
        public List<DocumentAccepted> acceptedDocuments { get; set; }
        public List<DocumentRejected> rejectedDocuments { get; set; }

    }
    public class DocumentAccepted
    {
        public string uuid { get; set; }
        public string longId { get; set; }
        public string internalId { get; set; }
    }
    public class DocumentRejected
    {
        public string internalId { get; set; }
        public Error error { get; set; }
    }
    public class Error
    {
        public string code { get; set; }
        public string message { get; set; }
        public string target { get; set; }
        public Error[] details { get; set; }
    }


    public class BaseMessageResponse
    {
        public bool Result;
        public string Message;
        public List<BaseError> Errors;
    }
    public class BaseError
    {
        public string ErrorMSG;
        public string ErrorCode;
    }
}