using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs
{
    public class Document
    {
        public List<DocumentElement> documents { get; set; }
    }

    public class SignedDocument
    {
        public List<SignedDocumentElement> documents { get; set; }
    }
    public class CreaditOrDebitDocument
    {
        public List<CreditOrDebitDocumentElement> documents { get; set; }
    }
    public class SignedCreaditOrDebitDocument
    {
        public List<SignedCreditOrDebitDocumentElement> documents { get; set; }
    }
}