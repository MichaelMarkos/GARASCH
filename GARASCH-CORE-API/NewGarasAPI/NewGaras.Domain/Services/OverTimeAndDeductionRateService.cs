using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.OverTimeAndDeductionRate;
using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class OverTimeAndDeductionRateService : IOverTimeAndDeductionRateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public OverTimeAndDeductionRateService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public BaseResponseWithId<long> AddOverTimeAndDeductionRate(OverTimeDeductionRateDto dto,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if (dto.BranchId != null && dto.BranchId != 0)
            {
                var branch = _unitOfWork.Branches.Find(x=>x.Id==dto.BranchId);
                if (branch != null)
                {
                    dto.To = dto.To.Add(new TimeSpan(0, 0, -1));
                    /*var rateCheck = _unitOfWork.OverTimeAndDeductionRates.FindAll(a =>
                ((dto.From <= a.From && dto.To >= a.To) ||
                (dto.From >= a.From && dto.From <= a.To) ||
                (dto.To >= a.From && dto.To <= a.To)) && dto.BranchId == a.BranchId && ((dto.Rate>0 && a.Rate>0)||(dto.Rate<0 && a.Rate<0))).FirstOrDefault();
                if (rateCheck != null)
                {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "there's is another Rate in this duration which is " + rateCheck.Rate + " between " + rateCheck.From.ToShortTimeString() + " and " + rateCheck.To.ToShortTimeString();
                        Response.Errors.Add(error);
                        return Response;
                }*/
                    var OAndD = _mapper.Map<OverTimeAndDeductionRate>(dto);
                    OAndD.CreationDate = DateTime.Now;
                    OAndD.ModifiedDate = DateTime.Now;
                    OAndD.CreatedBy = creator;
                    OAndD.ModifiedBy = creator;
                    var addedOAndD = _unitOfWork.OverTimeAndDeductionRates.Add(OAndD);
                    _unitOfWork.Complete();
                    Response.ID= addedOAndD.Id;
                    return Response;
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "This Branch Is not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "This Branch Id is Incorrect";
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> UpdateOverTimeAndDeductionRate(OverTimeDeductionRateDto dto, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if (dto.Id != null && dto.Id != 0)
            {
                if (dto.BranchId != null && dto.BranchId != 0)
                {
                    var branch = _unitOfWork.Branches.Find(x => x.Id == dto.BranchId);
                    if (branch != null)
                    {
                        if (dto.To.Second == 0)
                        {
                            dto.To = dto.To.Add(new TimeSpan(0, 0, -1));
                        }
                        var OAndD = _unitOfWork.OverTimeAndDeductionRates.Find(x => x.Id == dto.Id);
                        if (OAndD != null)
                        {
                            /*var rateCheck = _unitOfWork.OverTimeAndDeductionRates.FindAll(a =>
                ((dto.From <= a.From && dto.To >= a.To) ||
                (dto.From >= a.From && dto.From <= a.To) ||
                (dto.To >= a.From && dto.To <= a.To)) && dto.BranchId == a.BranchId && ((dto.Rate > 0 && a.Rate > 0) || (dto.Rate < 0 && a.Rate < 0)) && a.Id!=OAndD.Id).FirstOrDefault();
                            if (rateCheck != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err102";
                                error.ErrorMSG = "there's is another Rate in this duration which is " + rateCheck.Rate + " between " + rateCheck.From.ToShortTimeString() + " and " + rateCheck.To.ToShortTimeString();
                                Response.Errors.Add(error);
                                return Response;
                            }*/
                            _mapper.Map<OverTimeDeductionRateDto,OverTimeAndDeductionRate>(dto, OAndD);
                            OAndD.ModifiedDate = DateTime.Now;
                            OAndD.ModifiedBy = creator;
                            var UpdatedOAndD = _unitOfWork.OverTimeAndDeductionRates.Update(OAndD);
                            _unitOfWork.Complete();
                            Response.ID = UpdatedOAndD.Id;
                            return Response;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err104";
                            error.ErrorMSG = "This OverTime/Deduction Rate Is not Found";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "This Branch Is not Found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "This Branch Id is Incorrect";
                    Response.Errors.Add(error);
                    return Response;
                }
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "This OverTime/Deduction Rate Id is Incorrect";
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> DeleteOverTimeAndDeductionRate(long RateId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.ID = 0;
            if (RateId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "OverTime/Deduction Rate Id is required";
                Response.Errors.Add(error);
                return Response;
            }
            var rate = _unitOfWork.OverTimeAndDeductionRates.GetById(RateId);
            if (rate == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "This OverTime/Deduction Rate is not found";
                Response.Errors.Add(error);
                return Response;
            }
            _unitOfWork.OverTimeAndDeductionRates.Delete(rate);
            _unitOfWork.Complete();

            return Response;
        }

        public BaseResponseWithData<List<OverTimeDeductionRateDto>> GetOverTimeAndDeductionRate([FromHeader] int branchId)
        {
            BaseResponseWithData<List<OverTimeDeductionRateDto>> Response = new BaseResponseWithData<List<OverTimeDeductionRateDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if(branchId != 0)
            {
                var rates = _unitOfWork.OverTimeAndDeductionRates.FindAll(x => x.BranchId == branchId);
                var ratesdto = _mapper.Map<List<OverTimeDeductionRateDto>>(rates);
                _ = ratesdto.Select(a =>a.To = a.To.Add(new TimeSpan(0, 0, 1))).ToList();
                Response.Data = ratesdto;
                return Response;
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "This Branch Id is Incorrect";
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
