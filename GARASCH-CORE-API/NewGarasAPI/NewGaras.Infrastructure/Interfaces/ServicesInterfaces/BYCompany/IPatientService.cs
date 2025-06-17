using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.BYCompany;
using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany
{
    public interface IPatientService
    {
        public BaseResponseWithID AddNewPatient([FromForm] PatientDto request, long creator, string CompanyName);

        public BaseResponseWithID EditPatient([FromForm] PatientDto request, long creator, string CompanyName);
        public BaseResponseWithDataAndHeader<List<GetPatientDto>> GetPatients([FromHeader] string firstname, [FromHeader] string lastname, [FromHeader] DateTime? DOB, [FromHeader] string phone, [FromHeader] string IncuranceNo, [FromHeader] bool GetAll, [FromHeader] int CurrentPage = 1, [FromHeader] int PageSize = 1000);
        public BaseResponseWithData<GetPatientDetailsDto> GetPatientById([FromHeader] long patientId);
        public BaseResponseWithData<List<InsuranceDto>> GetUserInsurances([FromHeader] long patientId);
        public BaseResponseWithData<List<GetPatientsDDl>> GetUserPatients();
    }
}
