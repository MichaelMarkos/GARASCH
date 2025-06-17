using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.ProjectManagement;
using NewGarasAPI.Models.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment
{
    public interface IProjectChequeService
    {
        public BaseResponseWithId<long> AddNewCheque(ProjectChequeDto dto, string CompanyName, long creator);
        public BaseResponseWithDataAndHeader<GetAllProjectChequesResponse> GetAllProjectCheques(GetAllProjectChequesFilter filter, string CompanyName);

        public BaseResponseWithData<List<GetChequeStatusModel>> GetChequeStatusDDL();

        public BaseResponseWithData<GetProjectChequeDto> GetChequeById([FromHeader] long ChequeId);

        public BaseResponseWithId<long> UpdateCheque([FromForm] ProjectChequeDto dto, string CompanyName, long creator);
        public Task<BaseResponseWithId<long>> AddNewChequeList(List<ProjectChequeDto> dto, string CompanyName, long creator);

        public BaseResponseWithId<int> AddBankChequeTemplate(BankChequeTemplatedto dto, long creator, string CompanyName);
        public BaseResponseWithId<int> EditBankChequeTemplate(BankChequeTemplatedto dto, long creator, string CompanyName);

        public BaseResponseWithData<GetChequeTemplatesReponse> GetChequeTemplateList();
    }
}
