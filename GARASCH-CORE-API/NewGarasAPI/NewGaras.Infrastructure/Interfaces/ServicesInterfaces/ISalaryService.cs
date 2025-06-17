using NewGaras.Domain.DTO.Salary;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Contract;
using NewGaras.Infrastructure.DTO.Payment;
using NewGaras.Infrastructure.DTO.Salary;
using NewGaras.Infrastructure.DTO.Salary.AllowncesType;
using NewGaras.Infrastructure.DTO.Salary.SalaryAllownces;
using NewGaras.Infrastructure.DTO.Salary.SalaryDeduction;
using NewGaras.Infrastructure.DTO.Salary.SalaryDeductionTax;
using NewGaras.Infrastructure.DTO.Salary.SalaryTax;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Interfaces.ServicesInterfaces
{
    public interface ISalaryService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithId<long> AddSalary(AddSalaryDto salaryDto, long Creator);

        public BaseResponseWithData<GetSalaryDto>GetSalary(long HrUserId);
        public BaseResponseWithData<GetSalaryHistoryDto> GetSalaryHistoryForUser(long HrUserId);
        public BaseResponseWithId<long> EditSalary(AddSalaryDto salaryDto, long Creator);

        public BaseResponseWithId<long> ChangeSalaryForUser(AddSalaryDto salaryDto);

        public BaseResponseWithData<GetContractWithSalaryDto> GetContractWithSalary(long HrUserId);

        public BaseResponseWithId<long> AddAllowncesType(AddAllowanceTypeDto dto);

        public BaseResponseWithId<long> EditAllowncesType(EditAllownceTypeDto oldAllownceType);

        public BaseResponseWithData<EditAllownceTypeDto> GetAllowenceTypeById(long id);

        public Task<BaseResponseWithData<List<EditAllownceTypeDto>>> GetAllAllowenceTypes();

        public BaseResponseWithId<long> AddSalaryAllownces(AddSalaryAllownces addSalaryAllowncesDto);

        public BaseResponseWithData<GetSalaryDto> GetSalaryById(long id);

        public BaseResponseWithData<GetSalaryAllowncesDto> GetSalaryAllowncesById(int id);

        public BaseResponseWithData<List<GetSalaryAllowncesDto>> GetSalaryAllowncesList();

        public BaseResponseWithId<long> EditSalaryAllownces(EditSalaryAllowncesDto SalaryAllowncesDto);

        public BaseResponseWithData<List<GetSalaryAllowncesDto>> GetSalaryAllowncesListForHrUser(long HrUserID);

        public BaseResponseWithId<long> AddSalaryTax(AddSalaryTaxDto addSalaryTaxDto, long UserId);

        public BaseResponseWithData<GetSalaryTaxDto> GetSalaryTaxById(long salaryTaxId);

        public BaseResponseWithData<List<GetSalaryTaxDto>> GetSalaryTaxList();

        public BaseResponseWithId<long> EditSalaryTax(GetSalaryTaxDto getSalaryTaxDto);

        public BaseResponseWithId<int> AddDeductionType(AddDeductionTypeDto dto, long UserId);

        public BaseResponseWithData<EditDeductionTypeDto> GetDeductionTypeById(int id);

        public BaseResponseWithId<int> EditDeductionType(EditDeductionTypeDto dto);

        public BaseResponseWithData<List<EditDeductionTypeDto>> GetDeductionList();

        public BaseResponseWithId<long> AddSalaryDeductionTax(AddSalaryDeductionTax dto, long UserId);

        public BaseResponseWithId<long> EditSalaryDeductionTax(EditSalaryDeductionTax dto, long userId);

        public BaseResponseWithData<EditSalaryDeductionTaxDto> GetSalaryDudectionTaxById(long id);

        public BaseResponseWithData<List<EditSalaryDeductionTaxDto>> GetSalaryDudectionTaxList();
        public BaseResponseWithData<List<EditSalaryDeductionTaxDto>> GetSalaryDudectionTaxListForHrUser(long HrUserID);
        public BaseResponseWithData<List<SalaryType>> SalaryTypeDDL();
        public BaseResponseWithData<List<TaxType>> TaxTypeDDL();
        public BaseResponseWithId<int> AddPaymentForUser(AddPaymentList dto);
        public BaseResponseWithData<List<GetPaymentForUserDto>> GetPaymentForUser(long HrUserId);
        public BaseResponseWithId<int> EditPaymentForUser(EditPaymentDto dto);
        public BaseResponseWithId<long> DeletePaymentMethodForUSer(long SalaryId);
        public BaseResponseWithId<long> DeleteSalaryTax(long SalaryTaxId);
        public BaseResponseWithId<long> DeleteSalaryAllownces(int SalaryAllowncesId);
        public BaseResponseWithId<long> ArchiveSalaryTax(long id, bool IsArchived);
        public BaseResponseWithId<long> ArchiveSalaryAllownces(int id, bool IsArchived);
        public BaseResponseWithId<int> DeleteAllowncesType(int allowncesTypeId);
        public BaseResponseWithId<long> ArchiveAllowncesType(int id, bool IsArchived);
    }
}
