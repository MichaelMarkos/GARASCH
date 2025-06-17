using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Contract;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IContractService
    {
        public BaseResponseWithId<long> AddContract(AddContractDto contractDto,long Creator);

        public BaseResponseWithId<long> EditContract(AddContractDto contractDto, long Creator);

        public BaseResponseWithData<GetContractDto> GetContract(long HrUserId);
        public BaseResponseWithId<long> GetContractReportToUser(long UserID);

        public BaseResponse ExtendContracts(ITenantService tenant);
    }
}
