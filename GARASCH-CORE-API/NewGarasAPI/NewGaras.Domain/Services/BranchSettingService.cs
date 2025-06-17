using AutoMapper;
using Azure;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Branch;
using NewGaras.Infrastructure.DTO.BranchSetting;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = NewGarasAPI.Models.Common.Error;

namespace NewGaras.Domain.Services
{
    public class BranchSettingService : IBranchSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public BranchSettingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public BaseResponseWithData<List<long>> AddBranchSetting([FromForm] BranchSettingDto dto,long creator)
        {
            BaseResponseWithData<List<long>> Response = new BaseResponseWithData<List<long>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new List<long>();

            if(dto.branchId != null && dto.ApplySettingdForAll != null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "If ApplySettingdForAll is not null , then Branch ID should be null";
                Response.Errors.Add(error);
                return Response;
            }
            if(dto.branchId == null && dto.ApplySettingdForAll == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "If ApplySettingdForAll is  null , then Branch ID should be not null";
                Response.Errors.Add(error);
                return Response;
            }

            var settings = _unitOfWork.BranchSetting.GetAll();
            var branches = _unitOfWork.Branches.GetAll();

            if(dto.PayrollTo >31 || dto.PayrollTo<1)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Payroll To date cannot be more than 31 or less than 1";
                Response.Errors.Add(error);
                return Response;
            }
            if (dto.PayrollFrom > 31 || dto.PayrollFrom < 1)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Payroll from date cannot be more than 31 or less than 1";
                Response.Errors.Add(error);
                return Response;
            }
            if (dto.PayrollFrom!= dto.PayrollTo)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Payroll from date should be equale to payroll to date";
                Response.Errors.Add(error);
                return Response;
            }
            if (dto.branchId != null && dto.ApplySettingdForAll == null)
            {
                if (settings.Any(a => a.BranchId == dto.branchId))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "This Branch Already have a Setting";
                    Response.Errors.Add(error);
                    return Response;
                }
                var branch = _unitOfWork.Branches.GetById((int)dto.branchId);
                if (branch == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Branch is not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                var setting = _mapper.Map<BranchSetting>(dto);
                setting.CreatedBy = creator;
                setting.ModifiedBy = creator;
                setting.Active= true;
                setting.ModifiedDate = DateTime.Now;
                setting.CreationDate = DateTime.Now;
                setting.BranchId = (int)dto.branchId;
                var addedSetting =  _unitOfWork.BranchSetting.Add(setting);
                _unitOfWork.Complete();
                Response.Data.Add(addedSetting.Id);
                return Response;

            }
            if(dto.branchId == null && dto.ApplySettingdForAll == true)
            {
                foreach (var branch in branches)
                {
                    var checksetting = settings.Where(a => a.BranchId == branch.Id).FirstOrDefault();
                    if (checksetting != null )
                    { 
                        _mapper.Map<BranchSettingDto, BranchSetting>(dto, checksetting);
                        checksetting.ModifiedBy = creator;
                        checksetting.BranchId = branch.Id;
                        checksetting.ModifiedDate = DateTime.Now;
                        _unitOfWork.BranchSetting.Update(checksetting);
                        _unitOfWork.Complete();
                        Response.Data.Add(checksetting.Id);
                    }
                    else if(checksetting == null )
                    {
                        var setting = _mapper.Map<BranchSetting>(dto);
                        setting.CreatedBy = creator;
                        setting.ModifiedBy = creator;
                        setting.Active = true;
                        setting.ModifiedDate = DateTime.Now;
                        setting.CreationDate = DateTime.Now;
                        setting.BranchId = branch.Id;
                        var addedSetting = _unitOfWork.BranchSetting.Add(setting);
                        _unitOfWork.Complete();
                        Response.Data.Add(addedSetting.Id);
                    }
                }
                
            }
            if(dto.branchId == null && dto.ApplySettingdForAll == false)
            {
                foreach (var branch in branches)
                {
                    var checksetting = settings.Where(a => a.BranchId == branch.Id).FirstOrDefault();
                    if(checksetting == null)
                    {
                        var setting = _mapper.Map<BranchSetting>(dto);
                        setting.CreatedBy = creator;
                        setting.ModifiedBy = creator;
                        setting.Active = true;
                        setting.ModifiedDate = DateTime.Now;
                        setting.CreationDate = DateTime.Now;
                        setting.BranchId = branch.Id;
                        var addedSetting = _unitOfWork.BranchSetting.Add(setting);
                        _unitOfWork.Complete();
                        Response.Data.Add(addedSetting.Id);
                    }
                } 
            }
            return Response;
        }

        public BaseResponseWithData<List<long>> EditBranchSetting([FromForm] EditBranchSettingDto dto, long creator)
        {
            BaseResponseWithData<List<long>> Response = new BaseResponseWithData<List<long>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new List<long>();

            var settings = _unitOfWork.BranchSetting.GetAll();
            var branches = _unitOfWork.Branches.GetAll();

            var branchSettingDB = _unitOfWork.BranchSetting.GetById(dto.Id);

            /*if(branchSettingDB == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "branch Setting ID is not found";
                Response.Errors.Add(error);
                return Response;
            }
            if (dto.PayrollFrom == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                error.ErrorMSG = "Payroll From Day is required";
                Response.Errors.Add(error);
                return Response;
            }*/
            if(dto.PayrollTo == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err106";
                error.ErrorMSG = "Payroll To Day is required";
                Response.Errors.Add(error);
                return Response;
            }
            if (dto.branchID != null && dto.ApplySettingdForAll == null)
            {
                var branch = _unitOfWork.Branches.GetById((int)dto.branchID);
                if (branch == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Branch is not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (dto.PayrollTo > 31 || dto.PayrollTo < 1)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Pay roll To date cannot be more than 31 or less than 1";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (dto.PayrollFrom > 31 || dto.PayrollFrom < 1)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Pay roll from date cannot be more than 31 or less than 1";
                    Response.Errors.Add(error);
                    return Response;
                }
                var setting = settings.Where(a => a.Id == dto.Id).FirstOrDefault();
                _mapper.Map<EditBranchSettingDto, BranchSetting>(dto, setting);
                setting.ModifiedBy = creator;
                setting.ModifiedDate = DateTime.Now;
                _unitOfWork.BranchSetting.Update(setting);
                _unitOfWork.Complete();
                Response.Data.Add(setting.Id);
                return Response;

            }
            if (dto.branchID == null && dto.ApplySettingdForAll == true)
            {
                foreach (var branch in branches)
                {
                    var checksetting = settings.Where(a => a.BranchId == branch.Id).FirstOrDefault();
                    if (checksetting != null)
                    {
                        //_mapper.Map<EditBranchSettingDto, BranchSetting>(dto, checksetting);
                        //var setting = new BranchSetting();
                        checksetting.AllowOverTimeInWeekends = dto.AllowOverTimeInWeekends;
                        checksetting.AllowAutomaticOvertime = dto.AllowAutomaticOvertime;
                        checksetting.AllowDelayingDeduction = dto.AllowDelayingDeduction;
                        checksetting.PayrollFrom = dto.PayrollFrom;
                        checksetting.PayrollTo = dto.PayrollTo;
                        checksetting.ModifiedBy = creator;
                        checksetting.BranchId = branch.Id;
                        checksetting.ModifiedDate = DateTime.Now;
                        _unitOfWork.BranchSetting.Update(checksetting);
                        _unitOfWork.Complete();
                        Response.Data.Add(checksetting.Id);
                    }
                    else if (checksetting == null)
                    {
                        //var setting = _mapper.Map<BranchSetting>(dto);
                        var setting = new BranchSetting();
                        setting.AllowOverTimeInWeekends = dto.AllowOverTimeInWeekends;
                        setting.AllowAutomaticOvertime = dto.AllowAutomaticOvertime;
                        setting.AllowDelayingDeduction = dto.AllowDelayingDeduction;
                        //setting.BranchId = dto.branchID??0;             //get the branches without settings
                        setting.CreatedBy = creator;
                        setting.ModifiedBy = creator;
                        setting.PayrollFrom = dto.PayrollFrom;
                        setting.PayrollTo = dto.PayrollTo;
                        setting.Active = true;
                        setting.ModifiedDate = DateTime.Now;
                        setting.CreationDate = DateTime.Now;
                        setting.BranchId = branch.Id;
                        setting.BranchId = branch.Id;
                        var addedSetting = _unitOfWork.BranchSetting.Add(setting);
                        _unitOfWork.Complete();
                        Response.Data.Add(addedSetting.Id);
                    }
                }

            }
            if (dto.branchID == null && dto.ApplySettingdForAll == false)
            {
                foreach (var branch in branches)
                {
                    var checksetting = settings.Where(a => a.BranchId == branch.Id).FirstOrDefault();
                    if (checksetting == null)
                    {
                        var setting = _mapper.Map<BranchSetting>(dto);
                        setting.CreatedBy = creator;
                        setting.ModifiedBy = creator;
                        setting.Active = true;
                        setting.ModifiedDate = DateTime.Now;
                        setting.CreationDate = DateTime.Now;
                        setting.BranchId = branch.Id;
                        var addedSetting = _unitOfWork.BranchSetting.Add(setting);
                        _unitOfWork.Complete();
                        Response.Data.Add(addedSetting.Id);
                    }
                }
            }
            return Response;
        }

        public BaseResponseWithId<long> DeleteBranchSetting(long BranchSettingId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.ID = 0;

            if(BranchSettingId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Branch setting Id Is required";
                Response.Errors.Add(error);
                return Response;
            }
            var setting = _unitOfWork.BranchSetting.GetById(BranchSettingId);
            if(setting == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Branch setting not found";
                Response.Errors.Add(error);
                return Response;
            }

            _unitOfWork.BranchSetting.Delete(setting);
            _unitOfWork.Complete();
            return Response;
        }

        public BaseResponseWithData<GetBranchSettingDto> GetBranchSetting([FromHeader] int branchId)
        {
            BaseResponseWithData<GetBranchSettingDto> Response = new BaseResponseWithData<GetBranchSettingDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if(branchId != 0) 
            {
                var branch = _unitOfWork.Branches.Find(a=>a.Id== branchId);
                if(branch != null)
                {
                    var setting = _unitOfWork.BranchSetting.Find(a=>a.BranchId==branch.Id);
                    var dto = _mapper.Map<GetBranchSettingDto>(setting);
                    Response.Data = dto;
                    return Response;
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This Branch Is not found";
                    Response.Errors.Add(error);
                    return Response;
                }
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "This Branch Id Is Incorrect";
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
