using NewGaras.Infrastructure.Models.PoInvoice;
using NewGarasAPI.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IPoInvoiceService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public Task<GetPoInvoiceDataResponse> GetPoInvoiceData(long POID);
        public Task<BaseResponseWithID> AddEditPoInvoice(AddEditPoInvoice Request, long UserID);
        public Task<BaseResponseWithID> AddDirectPoInvoice(AddDirectPoInvoiceRequest Request, long UserID);
        public Task<BaseResponseWithID> AddNewPurchasePOExtraaFees(AddNewPurchasePOInvoiceExtraFeesRequest Request, long UserID);
        public Task<BaseMessageResponse> PurchasePOInvoicePDF(string CompanyName, [FromHeader] long POID, [FromHeader] bool? GeneratePDF);
        public Task<BaseMessageResponse> GetInvoiceDetailsPDF([FromHeader] long SalesOfferId);
    }
}
