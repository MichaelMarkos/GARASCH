using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesOfferDetailsResponse
    {
        public GetSalesOffer SalesOfferDetails {  get; set; }
        public List<GetSalesOfferProduct> SalesOfferProducts { get; set; }
        public List<Attachment> SalesOfferAttachments { get; set; }
        public List<GetTax> SalesOfferTaxes { get; set; }
        public List<GetSalesOfferDiscount> SalesOfferDiscounts { get; set; }
        public List<ExtraCost> SalesOfferExtraCosts { get; set; }
        public List<GetInvoiceData> SalesOfferInvoices { get; set; }
        public decimal TotalSalesOfferInvoicesAmount { get; set; }

        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
