using Azure;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.DTO.Salary;
using NewGaras.Domain.Interfaces.ServicesInterfaces;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Contract;
using NewGaras.Infrastructure.DTO.Payment;
using NewGaras.Infrastructure.DTO.Salary;
using NewGaras.Infrastructure.DTO.Salary.AllowncesType;
using NewGaras.Infrastructure.DTO.Salary.SalaryAllownces;
using NewGaras.Infrastructure.DTO.Salary.SalaryDeduction;
using NewGaras.Infrastructure.DTO.Salary.SalaryDeductionTax;
using NewGaras.Infrastructure.DTO.Salary.SalaryTax;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Common;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace NewGarasAPI.Controllers.HR
{
    [Route("HR/[controller]")]
    [ApiController]
    public class SalaryController : ControllerBase
    {
        private readonly ISalaryService _salaryService;
        private readonly IBranchService _branchService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public SalaryController(ISalaryService salaryService, IBranchService branchService,ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _salaryService = salaryService;
            _helper = new Helper.Helper();
            _branchService = branchService;
        }
        [HttpPost("AddSalary")]
        public BaseResponseWithId<long> AddSalary([FromForm] AddSalaryDto salaryDto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)

                {
                    response = _salaryService.AddSalary(salaryDto, validation.userID);

                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }

        }
        [HttpPost("EditSalary")]
        public BaseResponseWithId<long> EditSalary([FromForm] AddSalaryDto salaryDto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)

                {
                    response = _salaryService.EditSalary(salaryDto, validation.userID);

                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }

        }
        [HttpPost("ChangeSalaryForUser")]
        public BaseResponseWithId<long> ChangeSalaryForUser(AddSalaryDto salaryDto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _salaryService.Validation = validation;
                    response = _salaryService.ChangeSalaryForUser(salaryDto);

                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }

        }
        [HttpGet("GetSalary")]
        public BaseResponseWithData<GetSalaryDto> GetSalary([FromHeader] long HrUserId)
        {
            BaseResponseWithData<GetSalaryDto> response = new BaseResponseWithData<GetSalaryDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salaryService.GetSalary(HrUserId);

                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetSalaryHistoryForUser")]
        public BaseResponseWithData<GetSalaryHistoryDto> GetSalaryHistoryForUser([FromHeader] long HrUserId)
        {
            BaseResponseWithData<GetSalaryHistoryDto> response = new BaseResponseWithData<GetSalaryHistoryDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salaryService.GetSalaryHistoryForUser(HrUserId);

                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetContractWithSalary")]
        public BaseResponseWithData<GetContractWithSalaryDto> GetContractWithSalary([FromHeader] long HrUserId)
        {

            BaseResponseWithData<GetContractWithSalaryDto> response = new BaseResponseWithData<GetContractWithSalaryDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {

                    response = _salaryService.GetContractWithSalary(HrUserId);
                }
                return response;
            }

            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("AddAllowncesType")]
        public BaseResponseWithId<long> AddAllowncesType([FromForm] AddAllowanceTypeDto dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (dto.Type == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "please , insert a valid Allownce type";
                response.Errors.Add(error);

                return response;
            }
            if (dto.CurrencyID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "No Currency with this ID ,please insert a valid Currency ID";
                response.Errors.Add(error);

                return response;
            }
            if (dto.SalaryTypeID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "No SalaryType with this ID ,please insert a valid Salary Type ID";
                response.Errors.Add(error);

                return response;
            }
            if ((dto.Amount == 0 || dto.Amount == null) && (dto.Percentage == 0 || dto.Percentage == null))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "No Amount or Percentage were added ,please insert a at least one of them";
                response.Errors.Add(error);

                return response;
            }


            try
            {
                if (response.Result)
                {
                    var newAllownceType = _salaryService.AddAllowncesType(dto);
                    response.ID = newAllownceType.ID;
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("EditAllowncesType")]
        public BaseResponseWithId<long> EditAllowncesType([FromForm] EditAllownceTypeDto dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (dto.Type == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "please , insert a valid Allownce type";
                response.Errors.Add(error);

                return response;
            }
            if (dto.CurrencyID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "No Currency with this ID ,please insert a valid Currency ID";
                response.Errors.Add(error);

                return response;
            }
            if (dto.SalaryTypeID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "No SalaryType with this ID ,please insert a valid Salary Type ID";
                response.Errors.Add(error);

                return response;
            }
            if ((dto.Amount == 0 || dto.Amount == null) && (dto.Percentage == 0 || dto.Percentage == null))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "No Amount or Percentage were added ,please insert a at least one of them";
                response.Errors.Add(error);

                return response;
            }

            try
            {
                if (response.Result)
                {
                    var FoundOrNot = _salaryService.GetAllowenceTypeById(dto.Id);
                    if (FoundOrNot.Result)
                    {
                        var NewAllownceType = _salaryService.EditAllowncesType(dto);
                        if (!NewAllownceType.Result)
                        {
                            response.Errors.AddRange(NewAllownceType.Errors);
                            return response;
                        }
                        response.ID = NewAllownceType.ID;
                    }
                    else
                    {
                        response.Result = false;
                        response.Errors.AddRange(FoundOrNot.Errors);
                        return response;

                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetAllowncesType")]
        public BaseResponseWithData<EditAllownceTypeDto> GetAllowncesType([FromHeader] int id)
        {
            var response = new BaseResponseWithData<EditAllownceTypeDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var allowenceType = _salaryService.GetAllowenceTypeById(id);
                    if (!allowenceType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(allowenceType.Errors);
                        return response;
                    }
                    response.Data = allowenceType.Data;
                }
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

        [HttpGet("GetAllowncesTypeList")]
        public async Task<BaseResponseWithData<List<EditAllownceTypeDto>>> GetAllAllowncesType()
        {
            var response = new BaseResponseWithData<List<EditAllownceTypeDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var allowenceType = await _salaryService.GetAllAllowenceTypes();
                    if (!allowenceType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(allowenceType.Errors);
                        return response;
                    }
                    response.Data = allowenceType.Data;
                }
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

        [HttpPost("AddSalaryAllownces")]
        public BaseResponseWithId<long> AddSalaryAllownces([FromBody] AddSalaryAllownces addSalaryAllowncesDto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            var counter = 1;
            foreach (var salaryAllownces in addSalaryAllowncesDto.salaryAllowncesList)
            {

                if (salaryAllownces.SalaryID == 0 || salaryAllownces.SalaryID == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"please , insert a valid salary ID at {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (salaryAllownces.AllowncesTypeID == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"please , insert a valid Allownce type ID at {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (salaryAllownces.Amount == 0 && salaryAllownces.percentage == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"please , insert a valid Amount or Percentage at {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                //if (salaryAllownces.Amount == 0)
                //{
                //    response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err10";
                //    error.ErrorMSG = "please , insert a valid Amount";
                //    response.Errors.Add(error);

                //    return response;
                //}
                //if (salaryAllownces.percentage == 0 || salaryAllownces.percentage == null)
                //{
                //    response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err10";
                //    error.ErrorMSG = "please , insert a valid percentage type";
                //    response.Errors.Add(error);

                //    return response;
                //}
                //var salary = _salaryService.GetSalaryById(salaryAllownces.SalaryID);
                //if(!salary.Result)
                //{
                //    response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err10";
                //    error.ErrorMSG = "Salary Id does not exsist , please insert a valid Salary ID";
                //    response.Errors.Add(error);

                //    return response;
                //}
                //var allawonceType = _salaryService.GetAllowenceTypeById(salaryAllownces.AllowncesTypeID);
                //if (!allawonceType.Result)
                //{
                //    response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err10";
                //    error.ErrorMSG = "AllowncesType ID does not exsist , please insert a valid AllowncesType ID";
                //    response.Errors.Add(error);

                //    return response;
                //}
                counter++;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var newSalaryAllownce = _salaryService.AddSalaryAllownces(addSalaryAllowncesDto);
                    if (!newSalaryAllownce.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(newSalaryAllownce.Errors);
                        return response;
                    }
                    response.ID = newSalaryAllownce.ID;
                }
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

        [HttpGet("GetSalaryAllownces")]
        public BaseResponseWithData<GetSalaryAllowncesDto> GetSalaryAllownces([FromHeader] int salaryAllowncesId)
        {
            BaseResponseWithData<GetSalaryAllowncesDto> response = new BaseResponseWithData<GetSalaryAllowncesDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (salaryAllowncesId == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "The salary Id Is required";
                response.Errors.Add(err);
                return response;
            }
            try
            {
                if (response.Result)
                {
                    var salaryAllownces = _salaryService.GetSalaryAllowncesById(salaryAllowncesId);
                    response.Result = salaryAllownces.Result;
                    response.Errors = salaryAllownces.Errors;
                    response.Data = salaryAllownces.Data;

                }
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

        [HttpGet("GetSalaryAllowncesList")]
        public BaseResponseWithData<List<GetSalaryAllowncesDto>> GetSalaryAllowncesList()
        {
            BaseResponseWithData<List<GetSalaryAllowncesDto>> response = new BaseResponseWithData<List<GetSalaryAllowncesDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var salaryAllownces = _salaryService.GetSalaryAllowncesList();
                    response.Result = salaryAllownces.Result;
                    response.Errors = salaryAllownces.Errors;
                    response.Data = salaryAllownces.Data;

                }
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

        [HttpGet("GetSalaryAllowncesListForHrUser")]
        public BaseResponseWithData<List<GetSalaryAllowncesDto>> GetSalaryAllowncesListForHrUser([FromHeader] long HrUserID)
        {
            BaseResponseWithData<List<GetSalaryAllowncesDto>> response = new BaseResponseWithData<List<GetSalaryAllowncesDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var salaryAllownces = _salaryService.GetSalaryAllowncesListForHrUser(HrUserID);
                    response.Result = salaryAllownces.Result;
                    response.Errors = salaryAllownces.Errors;
                    response.Data = salaryAllownces.Data;

                }
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


        [HttpPost("EditSalaryAllownces")]
        public BaseResponseWithId<long> EditSalaryAllowncesType([FromBody] EditSalaryAllowncesDto salaryAllowncesDto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion
            var counter = 1;
            foreach (var salaryAllowance in salaryAllowncesDto.salaryAllowncesList)
            {

                #region validation
                if (salaryAllowance.SalaryId == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The Salary Id Field Is required at {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (salaryAllowance.AllowanceTypeID == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The AllowncesType ID Field Is required at {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (salaryAllowance.Amount == 0 && salaryAllowance.Percentage == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"please , insert a valid Amount or Percentage at {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                //if (salaryAllowance.Amount == 0)
                //{
                //    response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err10";
                //    error.ErrorMSG = "The Amount Field Is required";
                //    response.Errors.Add(error);

                //    return response;
                //}
                //if (salaryAllowance.Percentage == 0)
                //{
                //    response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err10";
                //    error.ErrorMSG = "The percentage Field Is required";
                //    response.Errors.Add(error);

                //    return response;
                //}
                //var salaryCheck = _salaryService.GetSalaryById(oladSalaryAllownces.SalaryID);
                //if (!salaryCheck.Result)
                //{
                //    response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err10";
                //    error.ErrorMSG = "The salary with this Id is not found";
                //    response.Errors.Add(error);

                //    return response;
                //}
                //var AllowcesTypeCheck = _salaryService.GetAllowenceTypeById(oladSalaryAllownces.AllowncesTypeID);
                //if (!AllowcesTypeCheck.Result)
                //{
                //    response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err10";
                //    error.ErrorMSG = "The AllownceType with this Id is not found";
                //    response.Errors.Add(error);

                //    return response;
                //}
                counter++;
                #endregion

            }

            try
            {
                if (response.Result)
                {
                    var editedDeductiontype = _salaryService.EditSalaryAllownces(salaryAllowncesDto);
                    if (!editedDeductiontype.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(editedDeductiontype.Errors);

                        return response;
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("AddSalaryTax")]
        public BaseResponseWithId<long> AddSalaryTax([FromForm] AddSalaryTaxDto salaryTaxDto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (salaryTaxDto.Percentage == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "please , insert a valid Percentage ";
                response.Errors.Add(error);

                return response;
            }
            if (salaryTaxDto.Min == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Min header is required";
                response.Errors.Add(error);

                return response;
            }
            if (salaryTaxDto.Max == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Mix header is required";
                response.Errors.Add(error);

                return response;
            }
            if (salaryTaxDto.BranchId != 0 && salaryTaxDto.BranchId != null)
            {
                var branch = _branchService.GetBranch(salaryTaxDto.BranchId ?? 0);
                if (!branch.Result)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "The Branch with this Id is not found";
                    response.Errors.Add(error);
                    return response;
                }

            }
            //if (salaryTaxDto.TaxTypeId == 0 || salaryTaxDto.TaxTypeId == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err10";
            //    error.ErrorMSG = "Tax type is required";
            //    response.Errors.Add(error);
            //    return response;
            //}
            if (salaryTaxDto.SalaryTypeId == 0 || salaryTaxDto.SalaryTypeId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Salary type is required";
                response.Errors.Add(error);
                return response;
            }

            if (string.IsNullOrWhiteSpace(salaryTaxDto.TaxTypeName))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Salary Tax Name is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var newSalaryTax = _salaryService.AddSalaryTax(salaryTaxDto, validation.userID);
                    if (!newSalaryTax.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(newSalaryTax.Errors);
                        return response;
                    }
                    response.ID = newSalaryTax.ID;
                }
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

        [HttpGet("GetSalaryTax")]
        public BaseResponseWithData<GetSalaryTaxDto> GetSalaryTax([FromHeader] int SalaryTaxId)
        {
            BaseResponseWithData<GetSalaryTaxDto> response = new BaseResponseWithData<GetSalaryTaxDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (SalaryTaxId == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "The salary Tax Id Is required";
                response.Errors.Add(err);
                return response;
            }
            try
            {
                if (response.Result)
                {
                    var salaryAllownces = _salaryService.GetSalaryTaxById(SalaryTaxId);
                    response.Result = salaryAllownces.Result;
                    response.Errors = salaryAllownces.Errors;
                    response.Data = salaryAllownces.Data;

                }
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

        [HttpGet("GetSalaryTaxList")]
        public BaseResponseWithData<List<GetSalaryTaxDto>> GetSalaryTaxList()
        {
            BaseResponseWithData<List<GetSalaryTaxDto>> response = new BaseResponseWithData<List<GetSalaryTaxDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var salaryAllownces = _salaryService.GetSalaryTaxList();
                    response.Result = salaryAllownces.Result;
                    response.Errors = salaryAllownces.Errors;
                    response.Data = salaryAllownces.Data;

                }
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

        [HttpPost("EditSalaryTax")]
        public BaseResponseWithId<long> EditSalaryTax([FromForm] GetSalaryTaxDto NewSalaryTax)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (NewSalaryTax.Id == 0 || NewSalaryTax.Id == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The ID Field Is required";
                response.Errors.Add(error);

                return response;
            }
            if (NewSalaryTax.Percentage == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The Percentage Field Is required";
                response.Errors.Add(error);

                return response;
            }
            if (NewSalaryTax.Min == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The Min Field Is required";
                response.Errors.Add(error);

                return response;
            }
            if (NewSalaryTax.Max == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The Max Field Is required";
                response.Errors.Add(error);

                return response;
            }

            if (string.IsNullOrWhiteSpace(NewSalaryTax.TaxTypeName))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Salary Tax Name is Required";
                response.Errors.Add(error);
                return response;
            }
            if (NewSalaryTax.BranchId != null)
            {
                var FoundOrNot = _branchService.GetBranch(NewSalaryTax.BranchId ?? 0);
                if (!FoundOrNot.Result)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "The Branch with this ID Is not found";
                    response.Errors.Add(error);

                    return response;
                }
            }
            try
            {
                if (response.Result)
                {
                    var editedSalaryTax = _salaryService.EditSalaryTax(NewSalaryTax);
                    if (!editedSalaryTax.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(editedSalaryTax.Errors);

                        return response;
                    }
                    response.ID = editedSalaryTax.ID;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("AddDeductionType")]
        public BaseResponseWithId<int> AddDeductionType([FromBody] AddDeductionTypeDto newDeductionType)
        {
            var response = new BaseResponseWithId<int>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (newDeductionType.Name == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The Name Field Is required";
                response.Errors.Add(error);

                return response;
            }
            try
            {
                if (response.Result)
                {
                    var newDeductionId = _salaryService.AddDeductionType(newDeductionType, validation.userID);
                    response.ID = newDeductionId.ID;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetDeductionTypeById")]
        public BaseResponseWithData<EditDeductionTypeDto> GetDeductionTypeById([FromHeader] int id)
        {
            BaseResponseWithData<EditDeductionTypeDto> response = new BaseResponseWithData<EditDeductionTypeDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (id == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "The Deduction Type Id Is required";
                response.Errors.Add(err);
                return response;
            }
            try
            {
                if (response.Result)
                {
                    var deductionType = _salaryService.GetDeductionTypeById(id);
                    response.Result = deductionType.Result;
                    response.Errors = deductionType.Errors;
                    response.Data = deductionType.Data;

                }
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

        [HttpPost("EditDeductionType")]
        public BaseResponseWithId<int> EditDeductionType([FromBody] EditDeductionTypeDto dto)
        {
            var response = new BaseResponseWithId<int>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (dto.Id == 0 || dto.Id == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The ID Field Is required";
                response.Errors.Add(error);

                return response;
            }
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The name Field Is required";
                response.Errors.Add(error);

                return response;
            }

            try
            {
                if (response.Result)
                {
                    var editedDeductiontype = _salaryService.EditDeductionType(dto);
                    if (!editedDeductiontype.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(editedDeductiontype.Errors);

                        return response;
                    }
                    response.ID = editedDeductiontype.ID;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetDeductionTypeList")]
        public BaseResponseWithData<List<EditDeductionTypeDto>> GetDeductionTypeList()
        {
            BaseResponseWithData<List<EditDeductionTypeDto>> response = new BaseResponseWithData<List<EditDeductionTypeDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var deductionType = _salaryService.GetDeductionList();
                    response.Result = deductionType.Result;
                    response.Errors = deductionType.Errors;
                    response.Data = deductionType.Data;

                }
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

        [HttpPost("AddSalaryDeductionTax")]
        public BaseResponseWithId<long> AddSalaryDeductionTax([FromBody] AddSalaryDeductionTax newSalaryDeductionTax)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion
            int counter = 1;
            foreach (var salaryDeductionTax in newSalaryDeductionTax.SalaryDeductionTaxList)
            {
                if (salaryDeductionTax.SalaryId == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The SalaryId Field Is required In Salary DeductionTax number : {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (string.IsNullOrWhiteSpace(salaryDeductionTax.TaxName))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The Tax Name Field Is required In Salary DeductionTax number : {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (salaryDeductionTax.Percentage == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The Percentage Field Is required In Salary DeductionTax number : {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (salaryDeductionTax.Amount == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The Amount Field Is required In Salary DeductionTax number : {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                counter++;
            }

            #region unnecessary vildation
            //if(newSalaryDeductionTax.SalaryId > 0)
            //{
            //    var salary = _salaryService.GetSalaryById(newSalaryDeductionTax.SalaryId);
            //    if(salary.Result == false)
            //    {
            //        response.Result = false;
            //        Error error = new Error();
            //        error.ErrorCode = "Err10";
            //        error.ErrorMSG = "The salary ID is not found , please enter a vaild Salary ID";
            //        response.Errors.Add(error);

            //        return response;

            //    }
            //}

            //if(newSalaryDeductionTax.DeductionTypeId != null)
            //{
            //    var deductionType = _salaryService.GetDeductionTypeById(newSalaryDeductionTax.DeductionTypeId??0);
            //    if(!deductionType.Result)
            //    {
            //        response.Result = false;
            //        Error error = new Error();
            //        error.ErrorCode = "Err10";
            //        error.ErrorMSG = "No DeductionType with this ID";
            //        response.Errors.Add(error);

            //        return response;
            //    }
            //}
            //if (newSalaryDeductionTax.SalaryTaxId != null)
            //{
            //    var deductionType = _salaryService.GetSalaryTaxById(newSalaryDeductionTax.SalaryTaxId ?? 0);
            //    if (!deductionType.Result)
            //    {
            //        response.Result = false;
            //        Error error = new Error();
            //        error.ErrorCode = "Err10";
            //        error.ErrorMSG = "No SalaryTax with this ID";
            //        response.Errors.Add(error);

            //        return response;
            //    }
            //}
            #endregion

            try
            {
                if (response.Result)
                {
                    var newDeductionId = _salaryService.AddSalaryDeductionTax(newSalaryDeductionTax, validation.userID);
                    if (!newDeductionId.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(newDeductionId.Errors);
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("EditSalaryDeductionTax")]
        public BaseResponseWithId<long> EditSalaryDeductionTax([FromBody] EditSalaryDeductionTax dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            int counter = 1;
            foreach (var salaryDeductionTax in dto.SalaryDeductionTaxList)
            {
                if (salaryDeductionTax.SalaryId == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The SalaryId Field Is required In Salary DeductionTax number : {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (string.IsNullOrWhiteSpace(salaryDeductionTax.TaxName))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The Tax Name Field Is required In Salary DeductionTax number : {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (salaryDeductionTax.Percentage == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The Percentage Field Is required In Salary DeductionTax number : {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                if (salaryDeductionTax.Amount == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = $"The Amount Field Is required In Salary DeductionTax number : {counter}";
                    response.Errors.Add(error);

                    return response;
                }
                counter++;
            }

            #region old validation
            //if (dto.Id == 0 || dto.Id == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err10";
            //    error.ErrorMSG = "The ID Field Is required";
            //    response.Errors.Add(error);

            //    return response;
            //}
            //if (dto.SalaryId == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err10";
            //    error.ErrorMSG = "The SalaryId Field Is required";
            //    response.Errors.Add(error);

            //    return response;
            //}
            //if (dto.SalaryId > 0)
            //{
            //    var salary = _salaryService.GetSalaryById(dto.SalaryId);
            //    if (salary.Result == false)
            //    {
            //        response.Result = false;
            //        Error error = new Error();
            //        error.ErrorCode = "Err10";
            //        error.ErrorMSG = "The salary ID is not found , please enter a vaild Salary ID";
            //        response.Errors.Add(error);

            //        return response;

            //    }
            //}
            //if (string.IsNullOrWhiteSpace(dto.TaxName))
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err10";
            //    error.ErrorMSG = "The Tax Name Field Is required";
            //    response.Errors.Add(error);

            //    return response;
            //}
            //if (dto.Percentage == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err10";
            //    error.ErrorMSG = "The Percentage Field Is required";
            //    response.Errors.Add(error);

            //    return response;
            //}
            //if (dto.Amount == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err10";
            //    error.ErrorMSG = "The Amount Field Is required";
            //    response.Errors.Add(error);

            //    return response;
            //}
            //if (dto.DeductionTypeId != null)
            //{
            //    var deductionType = _salaryService.GetDeductionTypeById(dto.DeductionTypeId ?? 0);
            //    if (!deductionType.Result)
            //    {
            //        response.Result = false;
            //        Error error = new Error();
            //        error.ErrorCode = "Err10";
            //        error.ErrorMSG = "No DeductionType with this ID";
            //        response.Errors.Add(error);

            //        return response;
            //    }
            //}
            //if (dto.SalaryTaxId != null)
            //{
            //    var deductionType = _salaryService.GetSalaryTaxById(dto.SalaryTaxId ?? 0);
            //    if (!deductionType.Result)
            //    {
            //        response.Result = false;
            //        Error error = new Error();
            //        error.ErrorCode = "Err10";
            //        error.ErrorMSG = "No SalaryTax with this ID";
            //        response.Errors.Add(error);

            //        return response;
            //    }
            //}

            #endregion

            try
            {
                if (response.Result)
                {
                    var editedDeductiontype = _salaryService.EditSalaryDeductionTax(dto, validation.userID);
                    if (!editedDeductiontype.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(editedDeductiontype.Errors);

                        return response;
                    }
                    response.ID = editedDeductiontype.ID;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetSalaryDeductionTaxById")]
        public BaseResponseWithData<EditSalaryDeductionTaxDto> GetSalaryDeductionTaxById([FromHeader] long SalaryDeductionTaxId)
        {
            BaseResponseWithData<EditSalaryDeductionTaxDto> response = new BaseResponseWithData<EditSalaryDeductionTaxDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (SalaryDeductionTaxId == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "The Salary Deduction Tax Id Is required";
                response.Errors.Add(err);
                return response;
            }
            try
            {
                if (response.Result)
                {
                    var deductionType = _salaryService.GetSalaryDudectionTaxById(SalaryDeductionTaxId);
                    response.Result = deductionType.Result;
                    response.Errors = deductionType.Errors;
                    response.Data = deductionType.Data;

                }
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

        [HttpGet("GetSalaryDudectionTaxList")]
        public BaseResponseWithData<List<EditSalaryDeductionTaxDto>> GetSalaryDudectionTaxById()
        {
            BaseResponseWithData<List<EditSalaryDeductionTaxDto>> response = new BaseResponseWithData<List<EditSalaryDeductionTaxDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var deductionType = _salaryService.GetSalaryDudectionTaxList();
                    response.Result = deductionType.Result;
                    response.Errors = deductionType.Errors;
                    response.Data = deductionType.Data;

                }
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

        [HttpGet("GetSalaryDudectionTaxListForHrUser")]
        public BaseResponseWithData<List<EditSalaryDeductionTaxDto>> GetSalaryDudectionTaxListForHrUser([FromHeader] long HrUserID)
        {
            BaseResponseWithData<List<EditSalaryDeductionTaxDto>> response = new BaseResponseWithData<List<EditSalaryDeductionTaxDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var deductionType = _salaryService.GetSalaryDudectionTaxListForHrUser(HrUserID);
                    response.Result = deductionType.Result;
                    response.Errors = deductionType.Errors;
                    response.Data = deductionType.Data;

                }
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

        [HttpGet("GetSalaryTypeDDL")]
        public BaseResponseWithData<List<SalaryType>> GetSalaryTypeDDL()
        {
            BaseResponseWithData<List<SalaryType>> response = new BaseResponseWithData<List<SalaryType>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    response = _salaryService.SalaryTypeDDL();

                }
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

        [HttpGet("GetTaxTypeDDL")]
        public BaseResponseWithData<List<TaxType>> GetTaxTypeDDL()
        {
            BaseResponseWithData<List<TaxType>> response = new BaseResponseWithData<List<TaxType>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    response = _salaryService.TaxTypeDDL();

                }
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

        //-------------------------------------Payment----------------------------------------

        [HttpPost("AddPaymentMethod")]
        public BaseResponseWithId<int> AddPaymentMethod([FromBody] AddPaymentList dto)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var payment = _salaryService.AddPaymentForUser(dto);
                    if (!payment.Result)
                    {
                        response.Errors.AddRange(payment.Errors);
                        return response;
                    }
                    response.ID = payment.ID;
                }
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

        [HttpGet("GetPaymentMethodList")]
        public BaseResponseWithData<List<GetPaymentForUserDto>> GetPaymentForUser([FromHeader] long HrUserId)
        {
            BaseResponseWithData<List<GetPaymentForUserDto>> response = new BaseResponseWithData<List<GetPaymentForUserDto>>
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var payment = _salaryService.GetPaymentForUser(HrUserId);
                    if (!payment.Result)
                    {
                        response.Errors.AddRange(response.Errors);
                        return response;
                    }
                    response.Data = payment.Data;
                }
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

        [HttpPost("EditPaymentMethod")]
        public BaseResponseWithId<int> EditPaymentForUser([FromBody] EditPaymentDto dto)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var payment = _salaryService.EditPaymentForUser(dto);
                    if (!payment.Result)
                    {
                        response.Errors.AddRange(payment.Errors);
                        return response;
                    }
                    response.ID = payment.ID;
                }
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


        [HttpPost("DeletePaymentMethodForUSer")]
        public BaseResponseWithId<long> DeletePaymentMethodForUSer([FromHeader]long SalaryId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (SalaryId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Salary Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {

                    var payment = _salaryService.DeletePaymentMethodForUSer(SalaryId);
                    if (!payment.Result)
                    {
                        response.Errors.AddRange(payment.Errors);
                        return response;
                    }
                    response.ID = payment.ID;
                }
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

        //----------------------------------Delete APIs---------------------------------------

        [HttpPost("DeleteSalaryTax")]
        public BaseResponseWithId<long> DeleteSalaryTax([FromForm] long SalaryTaxId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (SalaryTaxId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Salary Tax Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var SalaryTax = _salaryService.DeleteSalaryTax(SalaryTaxId);
                    if (!SalaryTax.Result)
                    {
                        response.Errors.AddRange(SalaryTax.Errors);
                        return response;
                    }
                    response.ID = SalaryTax.ID;
                }
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

        [HttpPost("DeleteSalaryAllownces")]
        public BaseResponseWithId<long> DeleteSalaryAllownces([FromForm] int SalaryAllowncesId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (SalaryAllowncesId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Salary Allownces Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var SalaryAllownces = _salaryService.DeleteSalaryAllownces(SalaryAllowncesId);
                    if (!SalaryAllownces.Result)
                    {
                        response.Errors.AddRange(SalaryAllownces.Errors);
                        return response;
                    }
                    response.ID = SalaryAllownces.ID;
                }
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

        [HttpPost("DeleteAllowncesType")]
        public BaseResponseWithId<long> DeleteAllowncesType([FromForm] int AllowncesTypeId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (AllowncesTypeId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Allownces Type Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var SalaryAllownces = _salaryService.DeleteAllowncesType(AllowncesTypeId);
                    if (!SalaryAllownces.Result)
                    {
                        response.Errors.AddRange(SalaryAllownces.Errors);
                        return response;
                    }
                    response.ID = SalaryAllownces.ID;
                }
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

        //----------------------------------Archived APIs-------------------------------------

        [HttpPost("ArchiveSalaryTax")]
        public BaseResponseWithId<long> ArchiveSalaryTax([FromForm]long Id, [FromForm]bool IsArchived)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (Id == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Salary Tax ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var project = _salaryService.ArchiveSalaryTax(Id, IsArchived);
                    if (!project.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(project.Errors);
                        return response;
                    }
                    response.ID = project.ID;
                }
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

        [HttpPost("ArchiveSalaryAllownces")]
        public BaseResponseWithId<long> ArchiveSalaryAllownces([FromForm] int Id, [FromForm] bool IsArchived)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (Id == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Salary Allownces ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var project = _salaryService.ArchiveSalaryAllownces(Id, IsArchived);
                    if (!project.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(project.Errors);
                        return response;
                    }
                    response.ID = project.ID;
                }
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

        [HttpPost("ArchiveAllowncesType")]
        public BaseResponseWithId<long> ArchiveAllowncesType([FromForm] int Id, [FromForm] bool IsArchived)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (Id == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Allownces type ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var project = _salaryService.ArchiveAllowncesType(Id, IsArchived);
                    if (!project.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(project.Errors);
                        return response;
                    }
                    response.ID = project.ID;
                }
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
