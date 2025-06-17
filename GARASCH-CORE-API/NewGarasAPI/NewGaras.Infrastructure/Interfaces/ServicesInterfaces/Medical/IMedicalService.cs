using NewGaras.Infrastructure.Models.InternalTicket;
using NewGaras.Infrastructure.Models.Medical;
using NewGaras.Infrastructure.Models.SalesOffer.InternalTicket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical
{
    public interface IMedicalService
    {
        public HearderVaidatorOutput Validation { get; set; }

        public BaseResponseWithId<long> AddNewPatient(NewPatientDto patient);

        public BaseResponseWithId<long> EditPatient(NewPatientDto patient);

        public BaseResponseWithData<GetPatientModel> GetPatient([FromHeader] long PatientId);

        public BaseResponseWithData<GetPatientsListModel> GetPatientsList([FromHeader] string SearchKey, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10);

        public BaseResponseWithData<List<TopData>> GetTopDoctors(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To, string OfferType, string OfferTypeReturn);

        public BaseResponseWithData<List<TopData>> GetTopDepartmentsOrCategories(int year, int month, int day, string type, string OfferType, string OfferTypeReturn);

        public BaseResponseWithData<InternalTicketDashboardSummary> GetMedicalDashboard(int year, int month, int day, DateTime? From, DateTime? To, string OfferType, string OfferTypeReturn);

        public BaseResponseWithData<GetInternalTicketsByDepartmentResponse> GetMedicalByTeams(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To, string OfferType, string OfferTypeReturn);

        public BaseResponseWithData<GetInternalTicketsByCategoryResponse> GetMedicalByCategories(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To, string OfferType, string OfferTypeReturn);
        public BaseResponseWithData<GetInternalTicketsByItemCategoryResponse> GetMedicalByItemCategories(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To, string OfferType, string OfferTypeReturn);
        public GetSalesOfferListForInternalTicketResponse GetSalesOfferListForMedical(GetSalesOfferInternalTicketListFilters filters, string OfferStatusParam);

        public string GetExcelColumnName(int columnIndex);

        public BaseResponseWithData<string> GetSalesOfferForStoreExcel(string From, string To, string UserId, string CompName, string OfferType, string OfferTypeReturn, long createdBy);

        public BaseResponseWithData<string> GetSalesOfferForStoreForAllUsersPDF(InternalticketheaderPdf header, long createdBy);

        public Task<BaseResponseWithId<long>> CreateDoctorUserAsync(AddDoctorUserDTO NewHrUser, long UserId, string CompanyName);
    }
}
