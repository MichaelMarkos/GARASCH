using AutoMapper;
using Azure;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using iTextSharp.text.pdf.parser.clipper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewGaras.Domain.DTO.Salary;
using NewGaras.Domain.Interfaces.ServicesInterfaces;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
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
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.User;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private HearderVaidatorOutput validation;

        public HearderVaidatorOutput Validation
        {
            get
            {
                return validation;
            }
            set
            {
                validation = value;
            }
        }
        public SalaryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public BaseResponseWithId<long> AddSalary(AddSalaryDto salaryDto, long Creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;
            if (salaryDto.ContractId == null || salaryDto.ContractId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Contract Id";
                response.Errors.Add(error);
                return response;
            }
            var contract = _unitOfWork.Contracts.Find(x => x.Id == salaryDto.ContractId);

            if (contract == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Contract not Found";
                response.Errors.Add(error);
                return response;
            }
            /*var salary = _unitOfWork.Salaries.FindAll(a => a.ContractId == contract.Id).FirstOrDefault();
            if (salary != null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "There's Already A salary for this Contract";
                response.Errors.Add(error);
                return response;
            }*/
            if (salaryDto.HrUserId == 0 || salaryDto.HrUserId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "you Must Enter The Hr User Id";
                response.Errors.Add(error);
                return response;
            }
            var hruser = _unitOfWork.HrUsers.Find(x => x.Id == salaryDto.HrUserId);
            if (hruser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Employee Not Found";
                response.Errors.Add(error);
                return response;
            }
            var Salary = _mapper.Map<Salary>(salaryDto);
            if (Salary != null)
            {
                Salary.CreatedDate = DateTime.Now;
                Salary.ModifiedDate = DateTime.Now;
                Salary.CreatedBy = Creator;
                Salary.ModifiedBy = Creator;
                Salary.IsCurrent = true;
                var AddedSalary = _unitOfWork.Salaries.Add(Salary);
                _unitOfWork.Complete();
                response.ID = AddedSalary.Id;

            }
            return response;

        }

        public BaseResponseWithId<long> EditSalary(AddSalaryDto salaryDto, long Creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;
            if (salaryDto.ContractId == null || salaryDto.ContractId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Contract Id";
                response.Errors.Add(error);
                return response;
            }
            if (salaryDto.Id == null || salaryDto.Id == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Salary Id";
                response.Errors.Add(error);
                return response;
            }
            var contract = _unitOfWork.Contracts.Find(x => x.Id == salaryDto.ContractId);

            if (contract == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Contract not Found";
                response.Errors.Add(error);
                return response;
            }
            var salary = _unitOfWork.Salaries.FindAll(a => a.Id == salaryDto.Id).FirstOrDefault();
            if (salary == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "salary not found";
                response.Errors.Add(error);
                return response;
            }
            if (salaryDto.HrUserId == 0 || salaryDto.HrUserId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "you Must Enter The Hr User Id";
                response.Errors.Add(error);
                return response;
            }
            var hruser = _unitOfWork.HrUsers.Find(x => x.Id == salaryDto.HrUserId);
            if (hruser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Employee Not Found";
                response.Errors.Add(error);
                return response;
            }
            _mapper.Map<AddSalaryDto, Salary>(salaryDto, salary);
            if (salary != null)
            {

                salary.ModifiedDate = DateTime.Now;
                salary.ModifiedBy = Creator;
                var AddedSalary = _unitOfWork.Salaries.Update(salary);
                _unitOfWork.Complete();
                response.ID = AddedSalary.Id;

            }
            return response;

        }


        public BaseResponseWithId<long> ChangeSalaryForUser(AddSalaryDto salaryDto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;

            if (salaryDto.ContractId == null || salaryDto.ContractId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Contract Id";
                response.Errors.Add(error);
                return response;
            }
            if (salaryDto.Id == null || salaryDto.Id == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Salary Id";
                response.Errors.Add(error);
                return response;
            }

            var contract = _unitOfWork.Contracts.Find(x => x.Id == salaryDto.ContractId);

            if (contract == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Contract not Found";
                response.Errors.Add(error);
                return response;
            }
            var salary = _unitOfWork.Salaries.FindAll(a => a.Id == salaryDto.Id, includes: new[] { "SalaryAllownces", "SalaryDeductionTaxes" }).FirstOrDefault();
            if (salary == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "salary not found";
                response.Errors.Add(error);
                return response;
            }
            salary.To = DateTime.Now;
            salary.IsCurrent = false;
            _unitOfWork.Complete();
            var Allowences = salary.SalaryAllownces.ToList();
            var Taxes = salary.SalaryDeductionTaxes.ToList();

            if (salaryDto.HrUserId == 0 || salaryDto.HrUserId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "you Must Enter The Hr User Id";
                response.Errors.Add(error);
                return response;
            }
            var hruser = _unitOfWork.HrUsers.Find(x => x.Id == salaryDto.HrUserId);
            if (hruser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Employee Not Found";
                response.Errors.Add(error);
                return response;
            }
            salaryDto.Id = null;
            var Salary = _mapper.Map<Salary>(salaryDto);
            if (Salary != null)
            {
                Salary.CreatedDate = DateTime.Now;
                Salary.ModifiedDate = DateTime.Now;
                Salary.CreatedBy = validation.userID;
                Salary.ModifiedBy = validation.userID;
                Salary.IsCurrent = true;
                Salary.From = DateTime.Now;
                Salary.To = null;
                var AddedSalary = _unitOfWork.Salaries.Add(Salary);
                _unitOfWork.Complete();
                response.ID = AddedSalary.Id;
                foreach (var a in Allowences)
                {
                    a.SalaryId = AddedSalary.Id;
                    _unitOfWork.Complete();
                }
                foreach (var a in Taxes)
                {
                    a.SalaryId = AddedSalary.Id;
                    _unitOfWork.Complete();
                }
            }
            return response;
        }
        public BaseResponseWithData<GetContractWithSalaryDto> GetContractWithSalary(long HrUserId)
        {
            BaseResponseWithData<GetContractWithSalaryDto> response = new BaseResponseWithData<GetContractWithSalaryDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.Data = new GetContractWithSalaryDto();
            
            
            var hrUser = _unitOfWork.HrUsers.GetById(HrUserId);
            if (hrUser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "User not Found";
                response.Errors.Add(error);
                return response;
            }
            var dbContract = _unitOfWork.Contracts.Find((x => x.HrUserId == HrUserId && x.IsCurrent == true), new[] { "ContactType", "ReportTo" });
            if (dbContract == null)
            {
                dbContract = _unitOfWork.Contracts.FindAll((x => x.HrUserId == HrUserId), new[] { "ContactType", "ReportTo" }).LastOrDefault();
            }
            if (dbContract != null)
            {
                var contract = new GetContractDto();
                contract = _mapper.Map<GetContractDto>(dbContract);
                var reporttos = _unitOfWork.ContractReportTos.FindAll(a => a.ContractId == contract.Id, includes: new[] { "ReportTo" });
                contract.FirstReportTo = reporttos.FirstOrDefault()?.ReportToId ?? 0;
                contract.FirstReportToName = (reporttos.FirstOrDefault()?.ReportTo?.FirstName + " " + reporttos.FirstOrDefault()?.ReportTo?.LastName) ?? null;
                if (reporttos.Count() > 1)
                {
                    contract.SecondReportTo = reporttos.LastOrDefault()?.ReportToId ?? 0;
                    contract.SecondReportToName = (reporttos.LastOrDefault()?.ReportTo?.FirstName + " " + reporttos.LastOrDefault()?.ReportTo?.LastName) ?? null;
                }
                response.Data.Contract = contract;
                var dbSalary = _unitOfWork.Salaries.FindAll(x => x.HrUserId == HrUserId && x.ContractId == contract.Id, includes: new[] { "Currency", "PaymentStrategy" }).OrderByDescending(a => a.CreatedDate).FirstOrDefault();
                if (dbSalary != null)
                {
                    var Salary = new GetSalaryDto();
                    Salary = _mapper.Map<GetSalaryDto>(dbSalary);
                    response.Data.Salary = Salary;
                }
            }
            return response;
        }

        public BaseResponseWithData<GetSalaryDto> GetSalary(long HrUserId)
        {
            BaseResponseWithData<GetSalaryDto> response = new BaseResponseWithData<GetSalaryDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            var Salary = new GetSalaryDto();
            var test = _unitOfWork.HrUsers.GetAll();
            var hrUser = _unitOfWork.HrUsers.GetById(HrUserId);
            if (hrUser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "User not Found";
                response.Errors.Add(error);
                return response;
            }
            var contract = _unitOfWork.Contracts.FindAll(x => x.HrUserId == HrUserId && x.IsCurrent).OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if (contract == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "no Current Contracts For This User";
                response.Errors.Add(error);
                return response;
            }
            var dbSalary = _unitOfWork.Salaries.FindAll(x => x.HrUserId == HrUserId && x.ContractId == contract.Id, includes: new[] { "Currency", "PaymentStrategy" }).OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if (dbSalary == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "No Current Contract is Found For this User";
                response.Errors.Add(error);
                return response;
            }
            Salary = _mapper.Map<GetSalaryDto>(dbSalary);
            response.Data = Salary;
            return response;
        }

        public BaseResponseWithData<GetSalaryHistoryDto> GetSalaryHistoryForUser(long HrUserId)
        {
            BaseResponseWithData<GetSalaryHistoryDto> response = new BaseResponseWithData<GetSalaryHistoryDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.Data = new GetSalaryHistoryDto();
            var hrUser = _unitOfWork.HrUsers.GetById(HrUserId);
            if (hrUser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "User not Found";
                response.Errors.Add(error);
                return response;
            }
            var dbSalary = _unitOfWork.Salaries.FindAll(x => x.HrUserId == HrUserId, includes: new[] { "Currency", "PaymentStrategy" }).OrderByDescending(a => a.CreatedDate).ToList();
            if (dbSalary == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "No Current Contract is Found For this User";
                response.Errors.Add(error);
                return response;
            }
            var salaryList = _mapper.Map<List<GetSalaryDto>>(dbSalary);
            response.Data.SalaryHistory = salaryList;
            response.Data.HrUserName = hrUser.FirstName + " " + hrUser.LastName;
            response.Data.HrUserId = hrUser.Id;
            return response;
        }

        public BaseResponseWithId<long> AddAllowncesType(AddAllowanceTypeDto dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            if (dto.Type == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "The Allownces Type is Empty , please add a valid value";
                response.Errors.Add(err);
                return response;
            }

            #region check In DB
            var Currency = _unitOfWork.Currencies.GetById(dto.CurrencyID);
            if (Currency == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "No Currency with this ID , please add a valid value";
                response.Errors.Add(err);
                return response;
            }
            var SalaryType = _unitOfWork.SalaryTypes.GetById(dto.SalaryTypeID);
            if (SalaryType == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "No SalaryType with this ID , please add a valid value";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                //AllowncesType newAllownces = new AllowncesType();
                //newAllownces.Type = dto.Type;
                var newAllownces = _mapper.Map<AllowncesType>(dto);

                var data = _unitOfWork.AllowncesTypes.Add(newAllownces);
                _unitOfWork.Complete();

                response.ID = data.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> EditAllowncesType(EditAllownceTypeDto dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check In DB
            var Currency = _unitOfWork.Currencies.GetById(dto.CurrencyID ?? 0);
            if (Currency == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "No Currency with this ID , please add a valid value";
                response.Errors.Add(err);
                return response;
            }
            var SalaryType = _unitOfWork.SalaryTypes.GetById(dto.SalaryTypeID ?? 0);
            if (SalaryType == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "No SalaryType with this ID , please add a valid value";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var allowncesType = _unitOfWork.AllowncesTypes.Find(a => a.Id == dto.Id);

                if (allowncesType == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This AllownceType ID is not found , please enter a valid ID";
                    response.Errors.Add(err);
                    return response;
                }

                allowncesType.Type = dto.Type;
                allowncesType.Amount = dto.Amount;
                allowncesType.Percentage = dto.Percentage;
                allowncesType.CurrencyId = dto.CurrencyID;
                allowncesType.SalaryTypeId = dto.SalaryTypeID;

                _unitOfWork.Complete();

                response.ID = allowncesType.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<EditAllownceTypeDto> GetAllowenceTypeById(long id)
        {
            BaseResponseWithData<EditAllownceTypeDto> response = new BaseResponseWithData<EditAllownceTypeDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            if (id == 0 || id == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "The Id of Allowence Type is required";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                var data = _unitOfWork.AllowncesTypes.Find(x => x.Id == id);
                if (data == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No AllowenceType with this Id";
                    response.Errors.Add(err);
                    return response;
                }
                EditAllownceTypeDto allownceType = new EditAllownceTypeDto();
                allownceType.Id = data.Id;
                allownceType.Type = data.Type;
                allownceType.Amount = data.Amount;
                allownceType.Percentage = data.Percentage;
                allownceType.CurrencyID = data.CurrencyId ?? 0;
                allownceType.SalaryTypeID = data.SalaryTypeId ?? 0;

                response.Data = allownceType;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithData<List<EditAllownceTypeDto>>> GetAllAllowenceTypes()
        {
            BaseResponseWithData<List<EditAllownceTypeDto>> response = new BaseResponseWithData<List<EditAllownceTypeDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var salaryTypeList = _unitOfWork.SalaryTypes.GetAll();
                var currrenciesList = _unitOfWork.Currencies.GetAll();

                var data = await _unitOfWork.AllowncesTypes.FindAllAsync(a => a.IsArchived == null || a.IsArchived == false);
                List<EditAllownceTypeDto> AllowenceTypeList = new List<EditAllownceTypeDto>();

                foreach (var allowenceType in data)
                {
                    EditAllownceTypeDto currentAllownceType = new EditAllownceTypeDto();
                    currentAllownceType.Id = allowenceType.Id;
                    currentAllownceType.Type = allowenceType.Type;
                    currentAllownceType.Percentage = allowenceType.Percentage;
                    currentAllownceType.Amount = allowenceType.Amount;
                    if (allowenceType.SalaryTypeId != null)
                    {
                        currentAllownceType.SalaryTypeID = allowenceType.SalaryTypeId;
                        currentAllownceType.SalaryTypeName = salaryTypeList.Where(a => a.Id == allowenceType.SalaryTypeId).FirstOrDefault().SalaryName;
                    }
                    if (allowenceType.CurrencyId != null)
                    {
                        currentAllownceType.CurrencyID = allowenceType.CurrencyId;
                        currentAllownceType.CurrencyName = currrenciesList.Where(a => a.Id == allowenceType.CurrencyId).FirstOrDefault().Name;
                    }
                    AllowenceTypeList.Add(currentAllownceType);
                }

                response.Data = AllowenceTypeList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> AddSalaryAllownces(AddSalaryAllownces addSalaryAllowncesDto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var newSalaryAllowances = new List<SalaryAllownce>();
                foreach (var salaryAllowance in addSalaryAllowncesDto.salaryAllowncesList)
                {

                    var salaryAllownces = _mapper.Map<SalaryAllownce>(salaryAllowance);

                    if (salaryAllownces != null)
                    {
                        newSalaryAllowances.Add(salaryAllownces);
                    }
                }
                _unitOfWork.SalaryAllownces.AddRange(newSalaryAllowances);
                _unitOfWork.Complete();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<GetSalaryDto> GetSalaryById(long id)
        {
            BaseResponseWithData<GetSalaryDto> response = new BaseResponseWithData<GetSalaryDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            if (id == 0 || id == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "The Id of Salary is required";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                var salary = _unitOfWork.Salaries.Find(a => a.Id == id);
                if (salary == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No salary with this Id";
                    response.Errors.Add(err);
                    return response;
                }
                var salaryData = _mapper.Map<GetSalaryDto>(salary);
                response.Data = salaryData;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<GetSalaryAllowncesDto> GetSalaryAllowncesById(int salaryAllowncesId)
        {
            BaseResponseWithData<GetSalaryAllowncesDto> response = new BaseResponseWithData<GetSalaryAllowncesDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var salaryAllownces = _unitOfWork.SalaryAllownces.Find(a => a.Id == salaryAllowncesId);
                if (salaryAllownces == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No salaryAllownces with this Id";
                    response.Errors.Add(err);
                    return response;
                }
                var salaryAllowncesData = _mapper.Map<GetSalaryAllowncesDto>(salaryAllownces);
                response.Data = salaryAllowncesData;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<List<GetSalaryAllowncesDto>> GetSalaryAllowncesList()
        {
            BaseResponseWithData<List<GetSalaryAllowncesDto>> response = new BaseResponseWithData<List<GetSalaryAllowncesDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var salaryAllowncesList = _unitOfWork.SalaryAllownces.GetAll();


                List<GetSalaryAllowncesDto> SalaryAllowncesDtoList = new List<GetSalaryAllowncesDto>();
                foreach (var item in salaryAllowncesList)
                {
                    var salaryAllowncesData = _mapper.Map<GetSalaryAllowncesDto>(item);
                    SalaryAllowncesDtoList.Add(salaryAllowncesData);

                }
                response.Data = SalaryAllowncesDtoList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        //------------------------------------------------------------------------------------------------------------------------
        public BaseResponseWithData<List<GetSalaryAllowncesDto>> GetSalaryAllowncesListForHrUser(long HrUserID)
        {
            BaseResponseWithData<List<GetSalaryAllowncesDto>> response = new BaseResponseWithData<List<GetSalaryAllowncesDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var salaryAllowncesList = _unitOfWork.Salaries.FindAll(a => a.HrUserId == HrUserID, new[] { "SalaryAllownces", "SalaryAllownces.AllowncesType" }).FirstOrDefault();

                if (salaryAllowncesList == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No salary with this Id";
                    response.Errors.Add(err);
                    return response;
                }

                var salaryAllowncesData = _mapper.Map<List<GetSalaryAllowncesDto>>(salaryAllowncesList.SalaryAllownces);
                response.Data = salaryAllowncesData;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> EditSalaryAllownces(EditSalaryAllowncesDto SalaryAllowncesDto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var IDsList = new List<long>();
                foreach (var salaryAllownce in SalaryAllowncesDto.salaryAllowncesList)
                {
                    long ID = salaryAllownce.ID;
                    IDsList.Add(ID);
                }

                var SalaryAllowanceListFromDB = _unitOfWork.SalaryAllownces.FindAll((a => IDsList.Contains(a.Id)));


                foreach (var salaryAllownce in SalaryAllowncesDto.salaryAllowncesList)  //Delete Old User
                {
                    var salaryAllowanceDB = SalaryAllowanceListFromDB.Where(a => a.Id == salaryAllownce.ID).FirstOrDefault();

                    if (salaryAllownce.Active == false && salaryAllowanceDB != null)
                    {
                        _unitOfWork.SalaryAllownces.Delete(salaryAllowanceDB);
                    }
                    if (salaryAllownce.ID > 0 && salaryAllownce.Active == true) //Edit already existed user
                    {
                        salaryAllowanceDB.SalaryId = salaryAllownce.SalaryId;
                        salaryAllowanceDB.AllowncesTypeId = salaryAllownce.AllowanceTypeID;
                        salaryAllowanceDB.Amount = salaryAllownce.Amount;
                        salaryAllowanceDB.Percentage = salaryAllownce.Percentage;

                    }
                    if (salaryAllownce.ID == 0 && salaryAllownce.Active == true)    //Add New User
                    {
                        var newSalaryAllowance = new SalaryAllownce();
                        newSalaryAllowance.SalaryId = salaryAllownce.SalaryId;
                        newSalaryAllowance.AllowncesTypeId = salaryAllownce.AllowanceTypeID;
                        newSalaryAllowance.Amount = salaryAllownce.Amount;
                        newSalaryAllowance.Percentage = salaryAllownce.Percentage;

                        _unitOfWork.SalaryAllownces.Add(newSalaryAllowance);
                    }


                }


                _unitOfWork.Complete();

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> AddSalaryTax(AddSalaryTaxDto addSalaryTaxDto, long UserId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var salarytype = _unitOfWork.SalaryTypes.GetById(addSalaryTaxDto.SalaryTypeId);
                if (salarytype == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err101";
                    err.errorCode = "Salary Type is not found";
                    response.Errors.Add(err);
                    return response;
                }
                //var taxtype = _unitOfWork.TaxTypes.GetById(addSalaryTaxDto.TaxTypeId);
                //if (taxtype == null)
                //{
                //    response.Result = false;
                //    Error err = new Error();
                //    err.ErrorCode = "Err101";
                //    err.errorCode = "Tax Type is not found";
                //    response.Errors.Add(err);
                //    return response;
                //}
                var NewSalaryTax = _mapper.Map<SalaryTax>(addSalaryTaxDto);

                if (NewSalaryTax != null)
                {
                    NewSalaryTax.TaxTypeName = NewSalaryTax.TaxTypeName.Trim();
                    NewSalaryTax.CreationDate = DateTime.Now;
                    NewSalaryTax.CreationBy = UserId;
                    var newSalaryAllownces = _unitOfWork.SalaryTaxs.Add(NewSalaryTax);
                    _unitOfWork.Complete();
                    response.ID = newSalaryAllownces.Id;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<GetSalaryTaxDto> GetSalaryTaxById(long salaryTaxId)
        {
            BaseResponseWithData<GetSalaryTaxDto> response = new BaseResponseWithData<GetSalaryTaxDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            if (salaryTaxId == 0 || salaryTaxId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "The Id of Salary Tax is required";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                var salaryTax = _unitOfWork.SalaryTaxs.FindAll(a => a.Id == salaryTaxId, includes: new[] { "SalaryType" }).FirstOrDefault();
                if (salaryTax == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No salary Tax with this Id";
                    response.Errors.Add(err);
                    return response;
                }
                var salaryData = _mapper.Map<GetSalaryTaxDto>(salaryTax);
                salaryData.SalayName = salaryTax.SalaryType?.SalaryName;
                salaryData.TaxTypeName = salaryTax.TaxTypeName;
                response.Data = salaryData;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<List<GetSalaryTaxDto>> GetSalaryTaxList()
        {
            BaseResponseWithData<List<GetSalaryTaxDto>> response = new BaseResponseWithData<List<GetSalaryTaxDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var salaryAllowncesList = _unitOfWork.SalaryTaxs.FindAll(a => true, includes: new[] { "SalaryType" });


                List<GetSalaryTaxDto> SalaryAllowncesDtoList = new List<GetSalaryTaxDto>();
                foreach (var item in salaryAllowncesList)
                {
                    var salaryAllowncesData = _mapper.Map<GetSalaryTaxDto>(item);
                    salaryAllowncesData.SalayName = item.SalaryType?.SalaryName;
                    salaryAllowncesData.TaxTypeName = item.TaxTypeName;
                    SalaryAllowncesDtoList.Add(salaryAllowncesData);

                }
                response.Data = SalaryAllowncesDtoList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> EditSalaryTax(GetSalaryTaxDto getSalaryTaxDto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {
                var salaryType = _unitOfWork.SalaryTaxs.Find(a => a.Id == getSalaryTaxDto.Id);

                if (salaryType == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This Salary Tax ID is not found , please enter a valid ID";
                    response.Errors.Add(err);
                    return response;
                }
                var salarytype = _unitOfWork.SalaryTypes.GetById(getSalaryTaxDto.SalaryTypeId);
                if (salarytype == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err101";
                    err.errorCode = "Salary Type is not found";
                    response.Errors.Add(err);
                    return response;
                }
                //var taxtype = _unitOfWork.TaxTypes.GetById(getSalaryTaxDto.TaxTypeId);
                //if (taxtype == null)
                //{
                //    response.Result = false;
                //    Error err = new Error();
                //    err.ErrorCode = "Err101";
                //    err.errorCode = "Tax Type is not found";
                //    response.Errors.Add(err);
                //    return response;
                //}
                salaryType.Percentage = getSalaryTaxDto.Percentage;
                salaryType.Min = getSalaryTaxDto.Min;
                salaryType.Max = getSalaryTaxDto.Max;
                salaryType.TaxTypeName = getSalaryTaxDto.TaxTypeName.Trim();
                salaryType.SalaryTypeId = getSalaryTaxDto.SalaryTypeId;
                if (getSalaryTaxDto.Active != null)
                {
                    salaryType.Active = getSalaryTaxDto.Active;
                }
                if (getSalaryTaxDto.BranchId != null)
                {
                    salaryType.BranchId = getSalaryTaxDto.BranchId;
                }

                _unitOfWork.Complete();

                response.ID = salaryType.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<int> AddDeductionType(AddDeductionTypeDto dto, long UserId)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (dto != null)
                {
                    DeductionType newDeductionType = new DeductionType();
                    newDeductionType.Name = dto.Name;
                    newDeductionType.Active = dto.Active;
                    newDeductionType.CreationDate = DateTime.Now;
                    newDeductionType.CreatedBy = UserId;
                    _unitOfWork.DeductionTypes.Add(newDeductionType);
                    _unitOfWork.Complete();
                    response.ID = newDeductionType.Id;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<int> EditDeductionType(EditDeductionTypeDto dto)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var oldDeductionType = _unitOfWork.DeductionTypes.Find(a => a.Id == dto.Id);
                if (oldDeductionType == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This Deduction Type ID is not found , please enter a valid ID";
                    response.Errors.Add(err);
                    return response;
                }
                oldDeductionType.Name = dto.Name;
                oldDeductionType.Active = dto.Active;
                _unitOfWork.Complete();

                response.ID = oldDeductionType.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<EditDeductionTypeDto> GetDeductionTypeById(int id)
        {
            BaseResponseWithData<EditDeductionTypeDto> response = new BaseResponseWithData<EditDeductionTypeDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            if (id == 0 || id == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "The Id of Deduction Type is required";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                var deductionType = _unitOfWork.DeductionTypes.Find(a => a.Id == id);
                if (deductionType == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No Deduction Type with this Id";
                    response.Errors.Add(err);
                    return response;
                }
                var deductionTypeData = new EditDeductionTypeDto()
                {
                    Id = deductionType.Id,
                    Name = deductionType.Name,
                    Active = deductionType.Active,
                };
                response.Data = deductionTypeData;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<List<EditDeductionTypeDto>> GetDeductionList()
        {
            BaseResponseWithData<List<EditDeductionTypeDto>> response = new BaseResponseWithData<List<EditDeductionTypeDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var salaryAllowncesList = _unitOfWork.DeductionTypes.GetAll();


                List<EditDeductionTypeDto> DeductionTypeDtoList = new List<EditDeductionTypeDto>();
                foreach (var item in salaryAllowncesList)
                {
                    var DeductionTypeData = _mapper.Map<EditDeductionTypeDto>(item);
                    DeductionTypeDtoList.Add(DeductionTypeData);

                }
                response.Data = DeductionTypeDtoList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> AddSalaryDeductionTax(AddSalaryDeductionTax dto, long UserId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region check in DB
            var loopCcounter = 0;
            foreach (var salaryDeductionTax in dto.SalaryDeductionTaxList)
            {
                var salary = _unitOfWork.Salaries.GetById(salaryDeductionTax.SalaryId);
                if (salary == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = $"No salary with this ID at SalaryDeductionTax number {loopCcounter}";
                    response.Errors.Add(error);
                    return response;
                }
                if (salaryDeductionTax.DeductionTypeId != null)
                {
                    var deductionType = _unitOfWork.DeductionTypes.GetById(salaryDeductionTax.DeductionTypeId ?? 0);
                    if (deductionType == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = $"No DeductionType with this ID at SalaryDeductionTax number {loopCcounter}";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                if (salaryDeductionTax.SalaryTaxId != null)
                {
                    var salaryTax = _unitOfWork.SalaryTaxs.GetById(salaryDeductionTax.SalaryTaxId ?? 0);
                    if (salaryTax == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = $"No SalaryTax with this ID at SalaryDeductionTax number {loopCcounter}";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                loopCcounter++;
            }
            #endregion
            try
            {
                if (dto != null)
                {
                    var salaryDeductionTaxList = _mapper.Map<List<SalaryDeductionTax>>(dto.SalaryDeductionTaxList);
                    foreach (var salaryDeductionTax in salaryDeductionTaxList)
                    {
                        salaryDeductionTax.CreatedBy = UserId;
                        salaryDeductionTax.CreationDate = DateTime.Now;
                        salaryDeductionTax.ModifiedBy = UserId;
                        salaryDeductionTax.ModifiedDate = DateTime.Now;
                    }

                    _unitOfWork.SalaryDeductionTaxs.AddRange(salaryDeductionTaxList);
                    _unitOfWork.Complete();
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> EditSalaryDeductionTax(EditSalaryDeductionTax dto, long userId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check in DB
            var loopCcounter = 0;
            foreach (var salaryDeductionTax in dto.SalaryDeductionTaxList)
            {
                var salary = _unitOfWork.Salaries.GetById(salaryDeductionTax.SalaryId);
                if (salary == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = $"No salary with this ID at SalaryDeductionTax number {loopCcounter}";
                    response.Errors.Add(error);
                    return response;
                }
                //var salaryTax = _unitOfWork.SalaryTaxs.GetById(salaryDeductionTax.SalaryTaxId??0);
                //if (salary == null)
                //{
                //    response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err102";
                //    error.ErrorMSG = $"No salary Tax with this ID at SalaryDeductionTax number {counter}";
                //    response.Errors.Add(error);
                //    return response;
                //}
                loopCcounter++;
            }
            #endregion

            try
            {
                var IDsList = new List<long>();
                foreach (var salaryTax in dto.SalaryDeductionTaxList)
                {
                    long ID = salaryTax.Id;
                    IDsList.Add(ID);
                }

                var SalaryDeductionTaxListFromDB = _unitOfWork.SalaryDeductionTaxs.FindAll((a => IDsList.Contains(a.Id)));

                int counter = 0;
                foreach (var salaryDeductionTax in dto.SalaryDeductionTaxList)
                {

                    //if(salaryDeductionTaxDB == null)
                    //{
                    //    response.Result = false;
                    //    Error err = new Error();
                    //    err.ErrorCode = "Err101";
                    //    err.errorCode = $"salary Deduction Tax number {counter+1} is not found";
                    //    response.Errors.Add(err);
                    //    return response;
                    //}
                    var salaryDeductionTaxDB = SalaryDeductionTaxListFromDB.Where(a => a.Id == salaryDeductionTax.Id).FirstOrDefault();

                    if (salaryDeductionTax.ActiveSalaryDeductionTax == false && salaryDeductionTaxDB != null)
                    {
                        _unitOfWork.SalaryDeductionTaxs.Delete(salaryDeductionTaxDB);
                    }
                    if (salaryDeductionTax.Id > 0 && salaryDeductionTax.ActiveSalaryDeductionTax == true)
                    {

                        var newData = _mapper.Map<EditSalaryDeductionTaxDto, SalaryDeductionTax>(salaryDeductionTax, salaryDeductionTaxDB);
                        newData.ModifiedDate = DateTime.Now;
                        newData.ModifiedBy = userId;
                    }
                    if (salaryDeductionTax.Id == 0 && salaryDeductionTax.ActiveSalaryDeductionTax == true)
                    {
                        var newSalaryDeductionTax = _mapper.Map<SalaryDeductionTax>(salaryDeductionTax);
                        newSalaryDeductionTax.Active = true;
                        newSalaryDeductionTax.CreatedBy = userId;
                        newSalaryDeductionTax.CreationDate = DateTime.Now;
                        newSalaryDeductionTax.ModifiedBy = userId;
                        newSalaryDeductionTax.ModifiedDate = DateTime.Now;

                        _unitOfWork.SalaryDeductionTaxs.Add(newSalaryDeductionTax);
                    }
                    counter++;
                }


                _unitOfWork.Complete();

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<EditSalaryDeductionTaxDto> GetSalaryDudectionTaxById(long SalaryDeductionTaxId)
        {
            var response = new BaseResponseWithData<EditSalaryDeductionTaxDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            if (SalaryDeductionTaxId == 0 || SalaryDeductionTaxId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "The Id of Salary Deduction Tax  is required";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                var SalaryDeductionTax = _unitOfWork.SalaryDeductionTaxs.FindAll(a => a.Id == SalaryDeductionTaxId).FirstOrDefault();
                if (SalaryDeductionTax == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No Salary Deduction Tax with this Id";
                    response.Errors.Add(err);
                    return response;
                }
                var salaryData = _mapper.Map<EditSalaryDeductionTaxDto>(SalaryDeductionTax);
                response.Data = salaryData;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<List<EditSalaryDeductionTaxDto>> GetSalaryDudectionTaxList()
        {
            BaseResponseWithData<List<EditSalaryDeductionTaxDto>> response = new BaseResponseWithData<List<EditSalaryDeductionTaxDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var SalaryDeductionTaxList = _unitOfWork.SalaryDeductionTaxs.GetAll();


                List<EditSalaryDeductionTaxDto> SalaryDeductionTypeDtoList = new List<EditSalaryDeductionTaxDto>();
                foreach (var item in SalaryDeductionTaxList)
                {
                    var DeductionTypeData = _mapper.Map<EditSalaryDeductionTaxDto>(item);
                    SalaryDeductionTypeDtoList.Add(DeductionTypeData);

                }
                response.Data = SalaryDeductionTypeDtoList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<List<SalaryType>> SalaryTypeDDL()
        {
            BaseResponseWithData<List<SalaryType>> response = new BaseResponseWithData<List<SalaryType>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var types = _unitOfWork.SalaryTypes.GetAll().ToList();
                response.Data = types;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<List<TaxType>> TaxTypeDDL()
        {
            BaseResponseWithData<List<TaxType>> response = new BaseResponseWithData<List<TaxType>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var types = _unitOfWork.TaxTypes.GetAll().ToList();
                response.Data = types;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        //--------------------------------salaryDudectionTax for each user --------------------------
        public BaseResponseWithData<List<EditSalaryDeductionTaxDto>> GetSalaryDudectionTaxListForHrUser(long HrUserID)
        {
            var response = new BaseResponseWithData<List<EditSalaryDeductionTaxDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            if (HrUserID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "The Id of Hr User is required";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                var salary = _unitOfWork.Salaries.FindAll(a => a.HrUserId == HrUserID, new[] { "SalaryDeductionTaxes" }).FirstOrDefault();
                if (salary == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No Salary with this Id";
                    response.Errors.Add(err);
                    return response;
                }
                var salaryDeductionTaxList = new List<SalaryDeductionTax>();

                foreach (var salaryDeductionTax in salary.SalaryDeductionTaxes)
                {
                    salaryDeductionTaxList.Add(salaryDeductionTax);
                }

                var salaryData = _mapper.Map<List<EditSalaryDeductionTaxDto>>(salaryDeductionTaxList);
                response.Data = salaryData;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        //-------------------------------------Payment APIs------------------------------------------
        public BaseResponseWithId<int> AddPaymentForUser(AddPaymentList dto)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>();
            response.Result = true;
            response.Errors = new List<Error>();


            var hrUser = _unitOfWork.HrUsers.FindAll(a => a.Id == dto.HrUserID).FirstOrDefault();
            if (hrUser == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "No HrUser witt this ID";
                response.Errors.Add(err);
                return response;
            }

            var paymentMethodList = _unitOfWork.PaymentMethods.GetAll();

            #region check in DB
            var counter = 0;
            foreach (var payment in dto.BankDetailsList)
            {
                var paymentMethod = paymentMethodList.Where(a => a.Id == dto.PaymentMethodID).FirstOrDefault();
                if (paymentMethod == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"No payment method with this ID at Payment number {counter}";
                    response.Errors.Add(err);
                    return response;
                }
                if (paymentMethod.Id == 1)
                {
                    if (string.IsNullOrEmpty(payment.BankName))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"No Bank Name at Payment number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                    if (string.IsNullOrEmpty(payment.AccountHolderFullName))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"No Account Holder FullName at Payment number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                    if (string.IsNullOrEmpty(payment.AccountNumber))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"No Account Number at Payment number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                counter++;
            }
            #endregion

            try
            {
                var salary = _unitOfWork.Salaries.FindAll(a => a.HrUserId == dto.HrUserID && a.Contract.IsCurrent, includes: new[] { "Contract" }).FirstOrDefault();

                //var salary = _unitOfWork.Salaries.FindAll(a => a.HrUserId == dto.HrUserID).FirstOrDefault();
                if (salary == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This User has No Salary from current contract";
                    response.Errors.Add(err);
                    return response;
                }
                var paymentList = new List<BankDetail>();
                if (dto.PaymentMethodID == 1 || dto.PaymentMethodID == 5)
                {
                    foreach (var pay in dto.BankDetailsList)
                    {
                        var payment = new BankDetail();
                        payment.HrUserId = dto.HrUserID;
                        payment.BankName = pay.BankName;
                        payment.AccountHolder = pay.AccountHolderFullName;
                        payment.AccountNumber = pay.AccountNumber;
                        payment.BankBranch = pay.BankBranch != null ? pay.BankBranch : "";
                        payment.ExpiryDate = pay.ExpiryDate != null ? pay.ExpiryDate : "";
                        paymentList.Add(payment);
                    }
                }
                salary.PaymentMethodId = dto.PaymentMethodID;
                _unitOfWork.BankDetails.AddRange(paymentList);
                _unitOfWork.Complete();
                //response.ID = dto.HrUserID;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<List<GetPaymentForUserDto>> GetPaymentForUser(long HrUserId)
        {
            BaseResponseWithData<List<GetPaymentForUserDto>> response = new BaseResponseWithData<List<GetPaymentForUserDto>>
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region check in DB
            var hrUser = _unitOfWork.HrUsers.FindAll(a => a.Id == HrUserId).FirstOrDefault();
            if (hrUser == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "No HrUser wit this ID";
                response.Errors.Add(err);
                return response;
            }
            #endregion


            try
            {
                var PaymentForUser = new List<GetPaymentForUserDto>();
                //var salary = _unitOfWork.Salaries.FindAll(a=>a.HrUserId == HrUserId).FirstOrDefault();
                var salary = _unitOfWork.Salaries.FindAll(a => a.HrUserId == HrUserId && a.Contract.IsCurrent, includes: new[] { "Contract" }).FirstOrDefault();
                if (salary.PaymentMethodId == 1 || salary.PaymentMethodId == 5)
                {
                    var paymentList = _unitOfWork.BankDetails.FindAll(a => a.HrUserId == HrUserId);
                    var payments = paymentList.Select(a => { var x = _mapper.Map<GetPaymentForUserDto>(a); x.PaymentMethodID = salary.PaymentMethodId; return x; }).ToList();
                    //_mapper.Map<List<GetPaymentForUserDto>>(paymentList);
                    //payments.FirstOrDefault().PaymentMethodID = salary.PaymentMethodId;
                    PaymentForUser.AddRange(payments);
                }
                else
                {
                    var tempPayment = new GetPaymentForUserDto();
                    tempPayment.HrUserID = HrUserId;
                    tempPayment.PaymentMethodID = salary.PaymentMethodId;
                    PaymentForUser.Add(tempPayment);
                }

                response.Data = PaymentForUser;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<int> EditPaymentForUser(EditPaymentDto dto)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region Validation
            var hrUser = _unitOfWork.HrUsers.FindAll(a => a.Id == dto.HrUserID).FirstOrDefault();
            if (hrUser == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "No HrUser with this ID";
                response.Errors.Add(err);
                return response;
            }

            var paymentMethodList = _unitOfWork.PaymentMethods.GetAll();
            var paymentMethod = paymentMethodList.Where(a => a.Id == dto.PaymentMethodID).FirstOrDefault();
            if (paymentMethod == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = $"No payment method wit this ID ";
                response.Errors.Add(err);
                return response;
            }
            if (paymentMethod.Id == 1)
            {
                if (string.IsNullOrEmpty(dto.BankName))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"No Bank Name ";
                    response.Errors.Add(err);
                    return response;
                }
                if (string.IsNullOrEmpty(dto.AccountHolderFullName))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"No Account Holder FullName";
                    response.Errors.Add(err);
                    return response;
                }
                if (string.IsNullOrEmpty(dto.AccountNumber))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"No Account Numberr";
                    response.Errors.Add(err);
                    return response;
                }

            }
            #endregion

            try
            {

                if (dto.PaymentMethodID == 1 || dto.PaymentMethodID == 5)
                {
                    if (dto.BankDetailsID != null && dto.BankDetailsID != 0)
                    {
                        var bankDetails = _unitOfWork.BankDetails.FindAll(a => a.Id == dto.BankDetailsID).FirstOrDefault();
                        if (bankDetails == null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = $"No bank or instapay Details with this ID";
                            response.Errors.Add(err);
                            return response;
                        }
                        bankDetails.HrUserId = dto.HrUserID;
                        bankDetails.BankName = dto.BankName;
                        bankDetails.AccountHolder = dto.AccountHolderFullName;
                        bankDetails.AccountNumber = dto.AccountNumber;
                        bankDetails.BankBranch = dto.BankBranch != null ? dto.BankBranch : "";
                        bankDetails.ExpiryDate = dto.ExpiryDate != null ? dto.ExpiryDate : "";

                        _unitOfWork.BankDetails.Update(bankDetails);
                    }
                    else
                    {
                        var payment = new BankDetail();
                        payment.HrUserId = dto.HrUserID;
                        payment.BankName = dto.BankName;
                        payment.AccountHolder = dto.AccountHolderFullName;
                        payment.AccountNumber = dto.AccountNumber;
                        payment.BankBranch = dto.BankBranch != null ? dto.BankBranch : "";
                        payment.ExpiryDate = dto.ExpiryDate != null ? dto.ExpiryDate : "";

                        _unitOfWork.BankDetails.Add(payment);
                    }
                    _unitOfWork.Complete();
                }
                var salary = _unitOfWork.Salaries.FindAll(a => a.HrUserId == dto.HrUserID && a.Contract.IsCurrent, includes: new[] { "Contract" }).FirstOrDefault();
                if (salary == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"No Salary for this user";
                    response.Errors.Add(err);
                    return response;
                }
                salary.PaymentMethodId = dto.PaymentMethodID;

                _unitOfWork.Salaries.Update(salary);
                _unitOfWork.Complete();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        //public BaseResponseWithData<>

        public BaseResponseWithId<long> DeletePaymentMethodForUSer(long SalaryId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;
            if (SalaryId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Salary Id";
                response.Errors.Add(error);
                return response;
            }
            var salary = _unitOfWork.Salaries.GetById(SalaryId);
            if (salary == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "salary not found";
                response.Errors.Add(error);
                return response;
            }

            if (salary.PaymentMethodId >= 2 && salary.PaymentMethodId <= 4)
            {
                var bankDetails = _unitOfWork.BankDetails.FindAll(a => a.HrUserId == salary.HrUserId);

                _unitOfWork.BankDetails.DeleteRange(bankDetails);
                _unitOfWork.Complete();
            }
            salary.PaymentMethodId = null;
            response.ID = salary.Id;
            _unitOfWork.Salaries.Update(salary);
            _unitOfWork.Complete();
            return response;
        }

        //-----------------------------------Delete APIs---------------------------------------------

        public BaseResponseWithId<long> DeleteSalaryTax(long SalaryTaxId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;
            if (SalaryTaxId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Salary Tax Id";
                response.Errors.Add(error);
                return response;
            }
            var salaryTax = _unitOfWork.SalaryTaxs.GetById(SalaryTaxId);
            if (salaryTax == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "salary Tax not found";
                response.Errors.Add(error);
                return response;
            }

            _unitOfWork.SalaryTaxs.Delete(salaryTax);
            _unitOfWork.Complete();


            response.ID = salaryTax.Id;
            return response;
        }

        public BaseResponseWithId<long> DeleteSalaryAllownces(int SalaryAllowncesId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;
            if (SalaryAllowncesId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Salary Allownces Id";
                response.Errors.Add(error);
                return response;
            }
            var salaryAllownces = _unitOfWork.SalaryAllownces.GetById(SalaryAllowncesId);
            if (salaryAllownces == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "salary Allownces not found";
                response.Errors.Add(error);
                return response;
            }

            _unitOfWork.SalaryAllownces.Delete(salaryAllownces);
            _unitOfWork.Complete();


            response.ID = salaryAllownces.Id;
            return response;
        }

        public BaseResponseWithId<int> DeleteAllowncesType(int allowncesTypeId)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>();
            response.Result = true;
            response.Errors = new List<Error>();

            var allowncesType = _unitOfWork.AllowncesTypes.GetById(allowncesTypeId);
            if (allowncesType == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Allownces Type with this Id";
                response.Errors.Add(error);
                return response;
            }

            _unitOfWork.AllowncesTypes.Delete(allowncesType);
            _unitOfWork.Complete();

            response.ID = allowncesType.Id;
            return response;
        }

        //-----------------------------------Archived APIs-------------------------------------------

        public BaseResponseWithId<long> ArchiveSalaryTax(long id, bool IsArchived)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check in DB
            var salaryTax = _unitOfWork.SalaryTaxs.GetById(id);
            if (salaryTax == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Salary Tax with this Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                var salaryDeductionTax = _unitOfWork.SalaryDeductionTaxs.FindAll(a => a.SalaryTaxId == id);
                foreach (var deductionTax in salaryDeductionTax)
                {
                    deductionTax.IsArchived = IsArchived;
                }

                salaryTax.IsArchived = IsArchived;

                _unitOfWork.Complete();


                response.ID = id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> ArchiveSalaryAllownces(int id, bool IsArchived)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check in DB
            var salaryAllownces = _unitOfWork.SalaryAllownces.GetById(id);
            if (salaryAllownces == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Salary Allownces with this Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                salaryAllownces.IsArchived = IsArchived;

                _unitOfWork.Complete();


                response.ID = id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> ArchiveAllowncesType(int id, bool IsArchived)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check in DB
            var allowncesType = _unitOfWork.AllowncesTypes.GetById(id);
            if (allowncesType == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Allownces type with this Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                allowncesType.IsArchived = IsArchived;

                _unitOfWork.Complete();


                response.ID = id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }




    }
}
