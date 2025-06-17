using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs
{
    public class DocumentElement
    {
        public Issuer issuer { get; set; }
        public Receiver receiver { get; set; }
        //public long invoiceID { get; set; }

        public string documentType { get; set; }
        public string documentTypeVersion { get; set; }
        public string dateTimeIssued { get; set; }
        public string taxpayerActivityCode { get; set; }
        public string internalID { get; set; }
        public string purchaseOrderReference { get; set; }
        public string purchaseOrderDescription { get; set; }
        public string salesOrderReference { get; set; }
        public string salesOrderDescription { get; set; }
        public string proformaInvoiceNumber { get; set; }
        public Payment payment { get; set; }
        public Delivery delivery { get; set; }
        public List<InvoiceLine> invoiceLines { get; set; }
        public decimal totalDiscountAmount { get; set; }
        public decimal totalSalesAmount { get; set; }
        public decimal netAmount { get; set; }
        public List<TaxTotal> taxTotals { get; set; }
        public decimal totalAmount { get; set; }
        public decimal extraDiscountAmount { get; set; }
        public decimal totalItemsDiscountAmount { get; set; }
        public List<Signature> signatures { get; set; }
    }
    public class SignedDocumentElement
    {
        public Issuer issuer { get; set; }
        public Receiver receiver { get; set; }
        //public long invoiceID { get; set; }

        public string documentType { get; set; }
        public string documentTypeVersion { get; set; }
        public string dateTimeIssued { get; set; }
        public string taxpayerActivityCode { get; set; }
        public string internalID { get; set; }
        public string purchaseOrderReference { get; set; }
        public string purchaseOrderDescription { get; set; }
        public string salesOrderReference { get; set; }
        public string salesOrderDescription { get; set; }
        public string proformaInvoiceNumber { get; set; }
        public Payment payment { get; set; }
        public Delivery delivery { get; set; }
        public List<InvoiceLine> invoiceLines { get; set; }
        public decimal totalDiscountAmount { get; set; }
        public decimal totalSalesAmount { get; set; }
        public decimal netAmount { get; set; }
        public List<TaxTotal> taxTotals { get; set; }
        public decimal totalAmount { get; set; }
        public decimal extraDiscountAmount { get; set; }
        public decimal totalItemsDiscountAmount { get; set; }
    }
    public class CreditOrDebitDocumentElement
    {
        public Issuer issuer { get; set; }
        public Receiver receiver { get; set; }
        //public long invoiceID { get; set; }
        public string documentType { get; set; }
        public string documentTypeVersion { get; set; }
        public string dateTimeIssued { get; set; }
        public string taxpayerActivityCode { get; set; }
        public string internalID { get; set; }
        public string purchaseOrderReference { get; set; }
        public string purchaseOrderDescription { get; set; }
        public string salesOrderReference { get; set; }
        public string salesOrderDescription { get; set; }
        public string proformaInvoiceNumber { get; set; }
        public List<string> references { get; set; }
        public Payment payment { get; set; }
        public Delivery delivery { get; set; }
        public List<InvoiceLine> invoiceLines { get; set; }
        public decimal totalDiscountAmount { get; set; }
        public decimal totalSalesAmount { get; set; }
        public decimal netAmount { get; set; }
        public List<TaxTotal> taxTotals { get; set; }
        public decimal totalAmount { get; set; }
        public decimal extraDiscountAmount { get; set; }
        public decimal totalItemsDiscountAmount { get; set; }
        public List<Signature> signatures { get; set; }
    }
    public class SignedCreditOrDebitDocumentElement
    {
        public Issuer issuer { get; set; }
        public Receiver receiver { get; set; }
        //public long invoiceID { get; set; }
        public string documentType { get; set; }
        public string documentTypeVersion { get; set; }
        public string dateTimeIssued { get; set; }
        public string taxpayerActivityCode { get; set; }
        public string internalID { get; set; }
        public string purchaseOrderReference { get; set; }
        public string purchaseOrderDescription { get; set; }
        public string salesOrderReference { get; set; }
        public string salesOrderDescription { get; set; }
        public string proformaInvoiceNumber { get; set; }
        public List<string> references { get; set; }
        public Payment payment { get; set; }
        public Delivery delivery { get; set; }
        public List<InvoiceLine> invoiceLines { get; set; }
        public decimal totalDiscountAmount { get; set; }
        public decimal totalSalesAmount { get; set; }
        public decimal netAmount { get; set; }
        public List<TaxTotal> taxTotals { get; set; }
        public decimal totalAmount { get; set; }
        public decimal extraDiscountAmount { get; set; }
        public decimal totalItemsDiscountAmount { get; set; }

    }
}