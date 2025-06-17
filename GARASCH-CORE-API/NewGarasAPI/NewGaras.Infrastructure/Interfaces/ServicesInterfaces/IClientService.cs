using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Client;
using NewGaras.Infrastructure.Models.Client.ClientsCardsStatistics;
using NewGaras.Infrastructure.Models.Client.Filters;
using NewGaras.Infrastructure.Models.Common;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IClientService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public GetClientsCardsData GetClientsDDL(GetClientsFilters filters, long creator, string companyname);
        public GetClientsCardsData GetClientsCardsDataResponse(GetClientsCardsFilters filters, long creator, string companyname);

        public AddNewClientScreenData GetClientScreenData(string CompanyName, int BranchId, int RoleId, long GroupId, string JobTitleId);

        public CountriesGovernoratesAreasDDLs GetCountriesGovernoratesAreasDDLs(string CompanyName);

        public SpecialitiesDDLResponse GetSpecialitiesDDL();

        public JobTitlesDDLResponse GetJobTitlesDDL();
        public Task<List<SelectDDL>> GetCurrenciesList(string CompanyName);

        public DeliveryAnshippingMethodsDDLResponse GetDeliveryAnshippingMethodsDDL();
        public BaseResponseWithId<long> AddNewClientContacts(ClientContactsData Request);
        public BaseResponseWithID AddNewClientContactPerson(ClientContactPersonData Request);
        public Task<BaseResponseWithID> AddClientTaxCard(ClientTaxCardData Request);
        public ClientsDetailsResponse GetClientsDetailsResponse(long ClientId);
        public Task<BaseResponseWithID> UpdateClassificationOfClient(UpdateClientClassRequest Request);
        public GetClientsCardsStatistics GetClientsCardsStatisticsResponse(GetClientsCardsStatisticsHeaders filters);
        public BaseResponseWithID AddNewClientConsultant(ClientConsultantData Request, long creator);
        public Task<ClientClassificationsResponse> GetClientClassifications();
        public void SaveAndValidateClientAttachment(UploadAttachment attachment, string type, string CompanyName, long ClientId, long CreatedBy);

        public BaseResponseWithId<long> ValidateClientAttachments(UploadAttachment attachment, int counter, string type);

        public BaseResponseWithId<long> AddClientAttachments(ClientAttachmentsData Request, string companyname);

        public BaseResponseWithData<List<SelectDDL>> GetClientsDropDown();

        public Task<GetClientDashboardResponse> GetClientDashboard([FromHeader] long ClientId);

        public CheckClientExistanceResponse CheckClientExistance(CheckClientExistanceFilters filters);

        public BaseResponseWithId<long> AddNewClientMobile(ClientMobileDataDto Request);

        public BaseResponseWithId<long> AddNewClientSpeciality(ClientSpecialityData Request);

        public BaseResponseWithId<long> AddNewClientLandLine(ClientLandLineDataDto Request);

        public BaseResponseWithId<long> AddNewClientFax(ClientFaxDataDto Request);
        public SelectDDLResponse GetClientList(GetClientListFilters filters);
        public BaseResponseWithID AddNewClient( ClientData request, long UserID);
        public BaseResponseWithId<long> DeleteClient(long ClientId);

        public Task<GetClientData> GetClientDataResponse([FromHeader] bool? OwnerCoProfile, [FromHeader] long? ClientId);
    }
}
