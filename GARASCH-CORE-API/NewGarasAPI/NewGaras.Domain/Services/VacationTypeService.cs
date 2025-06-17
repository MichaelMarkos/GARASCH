using AutoMapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.VacationType;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.HR;
using NewGarasAPI.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace NewGaras.Domain.Services
{
    public class VacationTypeService : IVacationTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public VacationTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public BaseResponseWithId<int> AddVacationType([FromForm] ContractLeaveSettingDto dto,long creator)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (dto.Id == null)
                {
                    // check if Leave Name exist before
                    var CheckContractLeave = _unitOfWork.ContractLeaveSetting.Count((x => x.Active == true && x.HolidayName.Trim().ToLower() == dto.HolidayName.Trim().ToLower()));
                    if (CheckContractLeave > 0 )
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "This Holiday has already exist before";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var contractLeave = _mapper.Map<ContractLeaveSetting>(dto);
                    if (dto.BalancePerMonth != 0 && dto.BalancePerMonth != null)
                    {
                        contractLeave.BalancePerMonth = dto.BalancePerMonth;
                        contractLeave.BalancePerYear = (decimal)dto.BalancePerMonth * (decimal)12;
                    }
                    else if (dto.BalancePerYear != 0 && dto.BalancePerYear != null)
                    {
                        contractLeave.BalancePerYear = dto.BalancePerYear;
                        contractLeave.BalancePerMonth = (decimal)dto.BalancePerYear / (decimal)12;
                    }
                    else if (dto.BalancePerYear == 0 && dto.BalancePerMonth == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Balance per month or balance per year should be added";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    contractLeave.CreationDate = DateTime.Now;
                    contractLeave.ModifiedDate = DateTime.Now;
                    contractLeave.CreatedBy = creator;
                    contractLeave.ModifiedBy = creator;
                    var addedVacation = _unitOfWork.ContractLeaveSetting.Add(contractLeave);
                    _unitOfWork.Complete();
                    Response.ID = addedVacation.Id;
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "You Cannot add An Existing Vacation Type";
                    Response.Errors.Add(error);
                    return Response;
                }
                
                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }
        public BaseResponseWithId<int> EditVacationType([FromForm] ContractLeaveSettingDto dto,long creator)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (dto.Id != null)
                {
                    var leave = _unitOfWork.ContractLeaveSetting.GetById((int)dto.Id);
                    if (leave != null)
                    {
                        _mapper.Map<ContractLeaveSettingDto, ContractLeaveSetting>(dto,leave);
                        if (dto.BalancePerMonth != 0 && dto.BalancePerMonth!=null)
                        {
                            leave.BalancePerMonth = dto.BalancePerMonth;
                            leave.BalancePerYear = (decimal)dto.BalancePerMonth * (decimal)12;
                        }
                        else if (dto.BalancePerYear != 0 && dto.BalancePerYear!=null)
                        {
                            leave.BalancePerYear = dto.BalancePerYear;
                            leave.BalancePerMonth = (decimal)dto.BalancePerYear / (decimal)12;
                        }
                        else if (dto.BalancePerYear == 0 && dto.BalancePerMonth == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "Balance per month or balance per year should be added";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        leave.ModifiedDate = DateTime.Now;
                        leave.ModifiedBy = creator;
                        var EditedVacation = _unitOfWork.ContractLeaveSetting.Update(leave);
                        _unitOfWork.Complete();
                        Response.ID = EditedVacation.Id;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "this vacation Type is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "You should provide the Vacation Type Id";
                    Response.Errors.Add(error);
                    return Response;
                }

                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<ContractLeaveSettingDto>> GetVacationTypesList()
        {
            BaseResponseWithData<List<ContractLeaveSettingDto>> Response = new BaseResponseWithData<List<ContractLeaveSettingDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                   var ListDB = _unitOfWork.ContractLeaveSetting.FindAll(a=>a.Archive==false || a.Archive==null);
                   var leaves = _mapper.Map<List<ContractLeaveSettingDto>>(ListDB);

                    Response.Data = leaves;
                


                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<ContractLeaveSettingDto> GetVacationType(int VacationTypeId)
        {
            BaseResponseWithData<ContractLeaveSettingDto> Response = new BaseResponseWithData<ContractLeaveSettingDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var ListDB = _unitOfWork.ContractLeaveSetting.GetById(VacationTypeId);
                if (ListDB == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Vacation Type Not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
                var leaves = _mapper.Map<ContractLeaveSettingDto>(ListDB);

                Response.Data = leaves;



                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }

        }
        public BaseResponseWithData<List<long>> EditVacationTypesForUser(List<VacationTypesForUser> list,long creator)
        {
            BaseResponseWithData<List<long>> Response = new BaseResponseWithData<List<long>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new List<long>();
            try
            {
                var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == list.FirstOrDefault().HrUserId && a.IsCurrent).FirstOrDefault();
                if (contract == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This User doesn't have a current Contract";
                    Response.Errors.Add(error);
                    return Response;
                }
                foreach (var vacationType in list)
                {
                    var vacation = _unitOfWork.ContractLeaveEmployees.GetById(vacationType.Id);
                    var oldBalance = vacation.Balance;
                    if (vacation != null)
                    {
                        _mapper.Map<VacationTypesForUser, ContractLeaveEmployee>(vacationType, vacation);
                        int months = ((contract.EndDate.Year - contract.StartDate.Year) * 12) + (contract.EndDate.Month - contract.StartDate.Month);

                        // If the end date is at least the last day of the month, count it as a full month
                        if (contract.EndDate.Day >= DateTime.DaysInMonth(contract.EndDate.Year, contract.EndDate.Month))
                        {
                            months += 1;
                        }
                        vacation.Balance = (int)(vacation.BalancePerMonth != null ? vacation.BalancePerMonth * months : 0);
                       /* if (vacationType.Balance != oldBalance)
                        {
                            vacation.BalancePerMonth = vacationType.Balance / months;
                        }
                        else
                        {
                            
                            
                        }*/
                        vacation.Remain = vacation.Balance - vacation.Used;
                        vacation.ModifiedDate = DateTime.Now;
                        vacation.ModifiedBy = creator;
                        var EditedVacation = _unitOfWork.ContractLeaveEmployees.Update(vacation);
                        _unitOfWork.Complete();
                        Response.Data.Add(vacation.Id);
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Vacation type not found for this user";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }
        

        public BaseResponseWithData<List<VacationTypesForUser>> GetVacationTypesForUser(long HrUserId)
        {
            BaseResponseWithData<List<VacationTypesForUser>> Response = new BaseResponseWithData<List<VacationTypesForUser>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == HrUserId && a.IsCurrent).FirstOrDefault();
                if (contract == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This User doesn't have Contract";
                    Response.Errors.Add(error);
                    return Response;
                }
                var vacationList = _unitOfWork.ContractLeaveEmployees.FindAll((a => a.HrUserId == HrUserId && a.ContractId == contract.Id), new[] { "ContractLeaveSetting" }).ToList();

                var returnedList = _mapper.Map<List<VacationTypesForUser>>(vacationList);
                Response.Data = returnedList;
                return Response;
            }
            catch (Exception ex) 
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<DDlVacationTypesForUser>> GetDDlVacationTypesForUser(long HrUserId)
        {
            BaseResponseWithData<List<DDlVacationTypesForUser>> Response = new BaseResponseWithData<List<DDlVacationTypesForUser>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == HrUserId && a.IsCurrent).FirstOrDefault();
                if (contract == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This User doesn't have Contract";
                    Response.Errors.Add(error);
                    return Response;
                }
                var vacationList = _unitOfWork.ContractLeaveEmployees.FindAll(a => a.HrUserId == HrUserId && a.Remain>0 && a.LeaveAllowed== "Allowed" && a.ContractId == contract.Id, includes: new[] { "ContractLeaveSetting"}).ToList();

                var returnedList = vacationList.Select(a=>new DDlVacationTypesForUser() {Id=a.ContractLeaveSetting.Id,Name=a.ContractLeaveSetting.HolidayName}).ToList();
                Response.Data = returnedList;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<int> DeleteLeaveType(int LeaveTypeId)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                if (LeaveTypeId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Leave type Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var leave = _unitOfWork.ContractLeaveSetting.Find(a=>a.Id== LeaveTypeId, includes: new[] { "Attendances", "LeaveRequests", "ContractLeaveEmployees" });
                if (leave == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Leave type Not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                /*var leaveemployee = _unitOfWork.ContractLeaveEmployees.FindAll(a => a.ContractLeaveSettingId == leave.Id).ToList();
                if(leaveemployee.Count >0)
                {
                    for (int i = 0; i < leaveemployee.Count; i++)
                    {
                        var leaveE  = leaveemployee[i];
                        _unitOfWork.ContractLeaveEmployees.Delete(leaveE);
                        _unitOfWork.Complete();
                    }
                }*/
                _unitOfWork.ContractLeaveSetting.Delete(leave);
                _unitOfWork.Complete();
                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<int> ArchiveLeaveType(int LeaveTypeId, bool Archive, long creator)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                if (LeaveTypeId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Leave type Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var leave = _unitOfWork.ContractLeaveSetting.Find(a => a.Id == LeaveTypeId, includes: new[] { "Attendances", "LeaveRequests", "ContractLeaveEmployees" });
                if (leave == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Leave type Not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                leave.Archive = Archive;
                leave.ModifiedBy = creator;
                leave.ModifiedDate = DateTime.Now;
                _unitOfWork.ContractLeaveSetting.Update(leave);
                _unitOfWork.Complete();
                Response.ID = leave.Id;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
