using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.InternalTicket;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGaras.Infrastructure.Models.SalesOffer.InternalTicket;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.AccountAndFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IInternalTicketService
    {
        public BaseResponseWithData<List<TopData>> GetTopDoctors(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To);

        public BaseResponseWithData<List<TopData>> GetTopDepartmentsOrCategories(int year, int month, int day, string type);

        public BaseResponseWithData<InternalTicketDashboardSummary> GetInternalTicketDashboard(int year, int month, int day, DateTime? From, DateTime? To);

        public BaseResponseWithData<GetInternalTicketsByDepartmentResponse> GetInternalTicketsByTeams(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To);

        public BaseResponseWithData<GetInternalTicketsByCategoryResponse> GetInternalTicketsByCategories(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To);

        public BaseResponseWithId<long> AddNewSalesOfferForInternalTicket(AddNewSalesOfferForInternalTicketRequest Request, string companyname, long creator);

        public GetSalesOfferListForInternalTicketResponse GetSalesOfferListForInternalTicket(GetSalesOfferInternalTicketListFilters filters, string OfferStatusParam);
        public GetSalesOfferDetailsForInternalTicketResponse GetSalesOfferDetailsForInternalTicket(long SalesOfferId);
        public Task<BaseResponseWithData<string>> GetSalesOfferTicketsForStoreForAllUsersPDF(InternalticketheaderPdf header, long createdBy);
        public BaseResponseWithData<GetInternalTicketsByItemCategoryResponse> GetInternalTicketsByItemCategories(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To);
        public BaseResponseWithData<string> GetSalesOfferTicketsForStore(string From, string To, string UserId, string CompName, long createdBy);

        //public BaseResponseWithId<long> AddNewSalesOfferForInternalTicket(AddNewSalesOfferForInternalTicketRequest Request, string companyname, long creator);

        //public GetSalesOfferListForInternalTicketResponse GetSalesOfferListForInternalTicket(GetSalesOfferInternalTicketListFilters filters, string OfferStatusParam);
        //public GetSalesOfferDetailsForInternalTicketResponse GetSalesOfferDetailsForInternalTicket(long SalesOfferId);
    }
}
