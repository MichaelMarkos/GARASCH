using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.BYCompany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany
{
    public interface IInsuranceCompanyNamesService
    {
        public BaseResponseWithData<List<InsuranceCompanyNameDto>> GetInsuranceCompanies();
        public BaseResponseWithId<long> AddInsuranceCompany(InsuranceCompanyNameDto dto,long creator);
    }
}
