using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferDetailsVM
    {
        public long ID { get; set; }
        public long POID { get; set; }
        public string InvoiceTo { get; set; }
        public string InvoiceClientPhoneNo { get; set; }
        public string ClientName { get; set; }
        public string Address { get; set; }
        public string InvoiceSerial { get; set; }
        public string SalesOfferSerial { get; set; }
        public string InvoiceDate { get; set; }
        public string SalesOfferDate { get; set; }
        public string AreaName { get; set; }
        public int BuildingNumber { get; set; }
        public string CountryName { get; set; }
        public string TaxCard { get; set; }
        public string FromCompanyName { get; set; }
        public string MainCompanyAddress { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public string GovernorateName { get; set; }
        public string TermsOfPayment { get; set; }
        public string RegistrationCard { get; set; }
        public decimal OfferAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal T1Amount { get; set; }
        public decimal T4Amount { get; set; }
        public decimal FinalOfferPrice { get; set; }

        public List<SalesOfferProductsVM> SalesOfferProductsList { get; set; }
    }
}
