using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.OverTimeAndDeductionRate;
using NewGaras.Infrastructure.DTO.VacationOverTimeAndDeductionRates;
using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class VacationOverTimeAndDeductionRateService : IVacationOverTimeAndDeductionRate
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public VacationOverTimeAndDeductionRateService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public BaseResponseWithData<List<long>> AddVacationOverTimeAndDeductionRateList(List<VacationOverTimeDeductionRateDto> dto, long creator)
        {
            BaseResponseWithData<List<long>> Response = new BaseResponseWithData<List<long>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new List<long>();
            foreach (var d in dto) {
                if (d.VacationDayId != null && d.VacationDayId != 0)
                {
                    var vacationDay = _unitOfWork.VacationDays.Find(x => x.Id == d.VacationDayId);
                    if (vacationDay != null)
                    {
                        d.To = d.To.Add(new TimeSpan(0, 0, -1));
                        /*var rateCheck = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a =>
                ((d.From <= a.From && d.To >= a.To) ||
                (d.From >= a.From && d.From <= a.To) ||
                (d.To >= a.From && d.To <= a.To)) && d.BranchId == a.VacationDay.BranchId && ((d.Rate > 0 && a.Rate > 0) || (d.Rate < 0 && a.Rate < 0)), includes: new[] {"VacationDay"}).FirstOrDefault();
                        if (rateCheck != null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err102";
                            error.ErrorMSG = "there's is another Rate in this duration which is " + rateCheck.Rate + " between " + rateCheck.From.ToShortTimeString() + " and " + rateCheck.To.ToShortTimeString();
                            Response.Errors.Add(error);
                            return Response;
                        }*/
                        var VOAndD = _mapper.Map<VacationOverTimeAndDeductionRate>(d);
                        VOAndD.CreationDate = DateTime.Now;
                        VOAndD.ModifiedDate = DateTime.Now;
                        VOAndD.CreatedBy = creator;
                        VOAndD.ModifiedBy = creator;
                        VOAndD.Active = true;
                        var addedOAndD = _unitOfWork.VacationOverTimeAndDeductionRates.Add(VOAndD);
                        _unitOfWork.Complete();
                        Response.Data.Add(addedOAndD.Id);
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "This Vacation Day Is not Found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "This Vacation Day Id is Incorrect";
                Response.Errors.Add(error);
                return Response;
            }
        }
            return Response;
        }

        public BaseResponseWithData<List<VacationOverTimeDeductionRateDto>> GetVacationOverTimeAndDeductionRate([FromHeader] long VacationDayId)
        {
            BaseResponseWithData<List<VacationOverTimeDeductionRateDto>> Response = new BaseResponseWithData<List<VacationOverTimeDeductionRateDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if (VacationDayId != 0)
            {
                var rates = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(x => x.VacationDayId == VacationDayId);
                var ratesdto = _mapper.Map<List<VacationOverTimeDeductionRateDto>>(rates);
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

        public BaseResponseWithId<long> UpdateVacationOverTimeAndDeductionRate(VacationOverTimeDeductionRateDto dto, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if (dto.Id != null && dto.Id != 0)
            {
                if (dto.VacationDayId != null && dto.VacationDayId != 0)
                {
                    var vacationDay = _unitOfWork.VacationDays.Find(x => x.Id == dto.VacationDayId);
                    if (vacationDay != null)
                    {
                        var VOAndD = _unitOfWork.VacationOverTimeAndDeductionRates.Find(x => x.Id == dto.Id);
                        if (VOAndD != null)
                        {
                            if (dto.To.Second == 0)
                            {
                                dto.To = dto.To.Add(new TimeSpan(0, 0, -1));
                            }
                            /*var rateCheck = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a =>
                ((dto.From <= a.From && dto.To >= a.To) ||
                            (dto.From >= a.From && dto.From <= a.To) ||
                (dto.To >= a.From && dto.To <= a.To)) && dto.BranchId == a.VacationDay.BranchId && ((dto.Rate > 0 && a.Rate > 0) || (dto.Rate < 0 && a.Rate < 0)) && a.Id != VOAndD.Id, includes: new[] {"VacationDay"}).FirstOrDefault();
                            if (rateCheck != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err102";
                                error.ErrorMSG = "there's is another Rate in this duration which is " + rateCheck.Rate + " between " + rateCheck.From.ToShortTimeString() + " and " + rateCheck.To.ToShortTimeString();
                                Response.Errors.Add(error);
                                return Response;
                            }*/
                            _mapper.Map<VacationOverTimeDeductionRateDto, VacationOverTimeAndDeductionRate>(dto, VOAndD);
                            VOAndD.ModifiedDate = DateTime.Now;
                            VOAndD.ModifiedBy = creator;
                            var UpdatedOAndD = _unitOfWork.VacationOverTimeAndDeductionRates.Update(VOAndD);
                            _unitOfWork.Complete();
                            Response.ID = UpdatedOAndD.Id;
                            return Response;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err104";
                            error.ErrorMSG = "This Vacation OverTime/Deduction Rate Is not Found";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "This Vacation Day Is not Found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "This Vacation Day Id is Incorrect";
                    Response.Errors.Add(error);
                    return Response;
                }
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "This Vacation OverTime/Deduction Rate Id is Incorrect";
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> DeleteVacationOverTimeAndDeductionRate(long RateId)
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
                error.ErrorMSG = "Vacation OverTime/Deduction Rate Id is required";
                Response.Errors.Add(error);
                return Response;
            }
            var rate = _unitOfWork.VacationOverTimeAndDeductionRates.GetById(RateId);
            if (rate == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "This Vacation OverTime/Deduction Rate is not found";
                Response.Errors.Add(error);
                return Response;
            }
            _unitOfWork.VacationOverTimeAndDeductionRates.Delete(rate);
            _unitOfWork.Complete();

            return Response;
        }
    }
}
