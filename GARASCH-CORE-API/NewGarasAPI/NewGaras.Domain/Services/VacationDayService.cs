using AutoMapper;
using Azure;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.VacationDay;
using NewGaras.Infrastructure.DTO.VacationOverTimeAndDeductionRates;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.HR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class VacationDayService : IVacationDayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public VacationDayService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public BaseResponseWithId<long> AddVacationDay([FromForm] AddVacationDayDto dto, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var CheckContractLeave = _unitOfWork.VacationDays.Count((x => x.Active == true && x.BranchId == dto.BranchId && x.EngName.Trim().ToLower() == dto.EngName.Trim().ToLower()));
            if (CheckContractLeave > 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "There is a vacation with this Name";
                Response.Errors.Add(error);
                return Response;
            }

            _unitOfWork.BeginTransaction();
            if (dto.Id == null)
            {
                var VacationCheck = _unitOfWork.VacationDays.FindAll(a =>
                ((dto.From <= a.From && dto.To >= a.To) ||
                (dto.From >= a.From && dto.From <= a.To) ||
                (dto.To >= a.From && dto.To <= a.To)) && dto.BranchId == a.BranchId).FirstOrDefault();
                if (VacationCheck == null)
                {
                    var vacationDay = _mapper.Map<VacationDay>(dto);
                    vacationDay.CreationDate = DateTime.Now;
                    vacationDay.ModifiedDate = DateTime.Now;
                    vacationDay.CreatedBy = creator;
                    vacationDay.ModifiedBy = creator;
                    vacationDay.Active = true;
                    var addedVacation = _unitOfWork.VacationDays.Add(vacationDay);
                    _unitOfWork.Complete();
                    Response.ID = addedVacation.Id;
                    if (dto.OverTimeDeductionRates != null && dto.OverTimeDeductionRates.Count > 0)
                    {
                        dto.OverTimeDeductionRates.ForEach(a => a.VacationDayId = addedVacation.Id);
                        var overtimes = dto.OverTimeDeductionRates;
                        var overTimesDb = _mapper.Map<List<VacationOverTimeAndDeductionRate>>(overtimes);
                        overTimesDb.ForEach(a => { a.CreationDate = DateTime.Now; a.ModifiedDate = DateTime.Now; a.CreatedBy = creator; a.ModifiedBy = creator; });
                        _unitOfWork.VacationOverTimeAndDeductionRates.AddRange(overTimesDb);
                        _unitOfWork.Complete();
                    }
                    _unitOfWork.CommitTransaction();
                    return Response;
                }
                else
                {
                    _unitOfWork.RollbackTransaction();
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "there's is another vacation in this duration which is " + VacationCheck.EngName + " between " + VacationCheck.From.ToShortDateString() + " and " + VacationCheck.To.ToShortDateString();
                    Response.Errors.Add(error);
                    return Response;
                }
            }
            else
            {
                _unitOfWork.RollbackTransaction();
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "You Can't Specify the Id of the Added vacation Day";
                Response.Errors.Add(error);
                return Response;
            }
        }
        public BaseResponseWithId<long> UpdateVacationDay([FromForm] AddVacationDayDto dto, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var CheckContractLeave = _unitOfWork.VacationDays.Count((x => x.Active == true && x.Id != dto.Id && x.BranchId == dto.BranchId && x.EngName.Trim().ToLower() == dto.EngName.Trim().ToLower()));
            if (CheckContractLeave > 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "There is a vacation with this Name";
                Response.Errors.Add(error);
                return Response;
            }

            _unitOfWork.BeginTransaction();
            if (dto.Id != null && dto.Id != 0)
            {
                var vacationDay = _unitOfWork.VacationDays.Find(x => x.Id == dto.Id);
                if (vacationDay != null)
                {

                    var VacationCheck = _unitOfWork.VacationDays.FindAll(a =>
                    ((dto.From <= a.From && dto.To >= a.To) ||
                    (dto.From >= a.From && dto.From <= a.To) ||
                    (dto.To >= a.From && dto.To <= a.To)) && dto.BranchId == a.BranchId && dto.Id != a.Id).FirstOrDefault();
                    if (VacationCheck == null)
                    {
                        _mapper.Map<AddVacationDayDto, VacationDay>(dto, vacationDay);
                        if (vacationDay.Active == false)
                        {
                            var rates = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.VacationDayId == vacationDay.Id).ToList();
                            _unitOfWork.VacationOverTimeAndDeductionRates.DeleteRange(rates);
                            _unitOfWork.Complete();
                            _unitOfWork.VacationDays.Delete(vacationDay);
                        }
                        else
                        {
                            vacationDay.ModifiedBy = creator;
                            vacationDay.ModifiedDate = DateTime.Now;
                            var updatedVacation = _unitOfWork.VacationDays.Update(vacationDay);
                            _unitOfWork.Complete();
                            Response.ID = updatedVacation.Id;
                            var overtimes = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.VacationDayId == updatedVacation.Id).ToList();

                            if (overtimes.Count() == dto.OverTimeDeductionRates.Count())
                            {
                                foreach (var a in overtimes)
                                {
                                    _mapper.Map<VacationOverTimeDeductionRateDto, VacationOverTimeAndDeductionRate>(dto.OverTimeDeductionRates[overtimes.IndexOf(a)], a);
                                    a.ModifiedDate = DateTime.Now;
                                    a.ModifiedBy = creator;
                                    _unitOfWork.VacationOverTimeAndDeductionRates.Update(a);
                                    _unitOfWork.Complete();
                                }
                                /*_mapper.Map<List<VacationOverTimeDeductionRateDto>, List<VacationOverTimeAndDeductionRate>>(dto.OverTimeDeductionRates, overtimes);
                                overtimes.ForEach(a => { a.ModifiedDate = DateTime.Now; a.ModifiedBy = creator; });
                                overtimes.ForEach(a => { _unitOfWork.VacationOverTimeAndDeductionRates.Update(a); });*/
                            }
                            else
                            {
                                _unitOfWork.VacationOverTimeAndDeductionRates.DeleteRange(overtimes);
                                var overtimesDb = _mapper.Map<List<VacationOverTimeAndDeductionRate>>(dto.OverTimeDeductionRates);
                                overtimesDb.ForEach(a => { a.CreatedBy = creator; a.ModifiedBy = creator; a.CreationDate = DateTime.Now; a.ModifiedDate = DateTime.Now; });
                                _unitOfWork.VacationOverTimeAndDeductionRates.AddRange(overtimesDb);
                            }
                        }
                        _unitOfWork.Complete();
                        _unitOfWork.CommitTransaction();
                        return Response;
                    }
                    else
                    {
                        _unitOfWork.RollbackTransaction();
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "there's is another vacation in this duration which is " + VacationCheck.EngName + " between " + VacationCheck.From.ToShortDateString() + " and " + VacationCheck.To.ToShortDateString();
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    _unitOfWork.RollbackTransaction();
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This Vacation Day Is not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
            }
            else
            {
                _unitOfWork.RollbackTransaction();
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "You Have To Specify the Id to Edit The Vacation Day";
                Response.Errors.Add(error);
                return Response;
            }
        }
        public BaseResponseWithData<GetVacationDayDto> GetVacationDay([FromHeader] long VacationDayId)
        {
            BaseResponseWithData<GetVacationDayDto> Response = new BaseResponseWithData<GetVacationDayDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if (VacationDayId != 0)
            {
                var vacationday = _unitOfWork.VacationDays.Find(a => a.Id == VacationDayId);
                if (vacationday != null)
                {
                    var dto = _mapper.Map<GetVacationDayDto>(vacationday);
                    dto.vacationOverTimes = _mapper.Map<List<VacationOverTimeDeductionRateDto>>(_unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.VacationDayId == dto.Id));
                    Response.Data = dto;
                    return Response;
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This Vacation Day Is not found";
                    Response.Errors.Add(error);
                    return Response;
                }
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "This Vacation Day Id Is Incorrect";
                Response.Errors.Add(error);
                return Response;
            }
        }
        public BaseResponseWithData<List<GetVacationDayDto>> GetVacationDayList([FromHeader] int branchId)
        {
            BaseResponseWithData<List<GetVacationDayDto>> Response = new BaseResponseWithData<List<GetVacationDayDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var vacations = _unitOfWork.VacationDays.FindAll(a => a.BranchId == branchId && (a.Archived==false || a.Archived==null));
            var dtos = _mapper.Map<List<GetVacationDayDto>>(vacations);
            foreach( var d in dtos )
            {
                var overtimes = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a=>a.VacationDayId==d.Id);
                d.vacationOverTimes = _mapper.Map<List<VacationOverTimeDeductionRateDto>>(overtimes);
            }
            Response.Data = dtos;
            return Response;
        }

        public BaseResponseWithData<List<GetGroupedVacationDays>> GetVacationDaysTree(int branchId) 
        {
            BaseResponseWithData<List<GetGroupedVacationDays>> Response = new BaseResponseWithData<List<GetGroupedVacationDays>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var list = new List<GetVacationDaysTreeDto>();
            var returnedList = new List<IGrouping<string,GetVacationDaysTreeDto>>();

            var vacations = _unitOfWork.VacationDays.FindAll(a => a.BranchId == branchId);
            foreach( var v in vacations)
            {
                var from = v.From;
                var to = v.To;
                while (to >= from)
                {
                    var day = new GetVacationDaysTreeDto()
                    {
                        Id = v.Id,
                        Day = DateOnly.FromDateTime(from),
                        DayName = from.DayOfWeek.ToString(),
                        VacationDayName = v.EngName
                    };
                    list.Add(day);
                    from=from.AddDays(1);
                }
            }
            returnedList = list.GroupBy(a => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(a.Day.Month) + " " + a.Day.Year).ToList();
            Response.Data = returnedList.Select(a=>new GetGroupedVacationDays() { Key = a.Key, VacationDays = [..a]}).ToList();
            return Response;
        }

        public BaseResponseWithId<long> ArchiveVacationDay(long VacationdayId,bool Archive,long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                if (VacationdayId == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err101";
                    err.ErrorMSG = "VacationDay Id Is required";
                    response.Errors.Add(err);
                    return response;
                }
                var vacationday = _unitOfWork.VacationDays.GetById(VacationdayId);
                if(vacationday == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err102";
                    err.ErrorMSG = "VacationDay is not found";
                    response.Errors.Add(err);
                    return response;
                }
                vacationday.Archived = Archive;
                vacationday.ModifiedBy = creator;
                vacationday.ModifiedDate = DateTime.Now;
                _unitOfWork.VacationDays.Update(vacationday);
                _unitOfWork.Complete();

                response.ID = VacationdayId;
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "Err10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public List<DateTime> GetDatesOfDayInMonth(int year, int month, DayOfWeek targetDay)
        {
            List<DateTime> dates = new List<DateTime>();

            // Get the first day of the month
            DateTime firstDayOfMonth = new DateTime(year, month, 1);

            // Calculate the first occurrence of the target day in the month
            int daysToAdd = ((int)targetDay - (int)firstDayOfMonth.DayOfWeek + 7) % 7;
            DateTime currentDate = firstDayOfMonth.AddDays(daysToAdd);

            // Loop through the month and add all occurrences of the target day
            while (currentDate.Month == month)
            {
                dates.Add(currentDate);
                currentDate = currentDate.AddDays(7); // Move to the next week
            }

            return dates;
        }

        public BaseResponseWithData<GetHolidaysOfMonthModel> GetHolidaysOfMonth([FromHeader] int BranchId, [FromHeader] int year, [FromHeader] int Month)
        {
            BaseResponseWithData<GetHolidaysOfMonthModel> Response = new BaseResponseWithData<GetHolidaysOfMonthModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                Response.Data = new GetHolidaysOfMonthModel();
                if (year == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "You have to enter year Value";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (Month == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "You have to enter Month Value";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (BranchId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "You have to enter BranchId Value";
                    Response.Errors.Add(error);
                    return Response;
                }
                var holidays = new List<HolidayModel>();
                var vacations = _unitOfWork.VacationDays.FindAll(a => a.BranchId == BranchId && ((a.From.Year==year && a.From.Month==Month) || (a.To.Year == year && a.To.Month == Month)) && (a.Archived == false || a.Archived == null));

                holidays = vacations.Select(a => new HolidayModel() { BranchId = a.BranchId,HolidayEnName = a.EngName,HolidayArName = a.ArName,DateFrom = a.From.ToString("yyyy-MM-dd"), DateTo = a.To.ToString("yyyy-MM-dd"),IsWeekEnd = false }).ToList();

                var WorkingDayIds = _unitOfWork.BranchSchedules.FindAll(a=>a.BranchId==BranchId).Select(a=>a.WeekDayId).ToList();
                var weekends = _unitOfWork.WeekDays.FindAll(a => !WorkingDayIds.Contains(a.Id)).ToList();

                foreach(var weekend in weekends)
                {
                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), weekend.Day);
                    var dates = GetDatesOfDayInMonth(year, Month, day);
                    var list = dates.Select(a=>new HolidayModel() { HolidayArName = weekend.Day,HolidayEnName = weekend.Day,DateFrom = a.Date.ToString("yyyy-MM-dd"),DateTo = a.Date.ToString("yyyy-MM-dd"),BranchId = BranchId,IsWeekEnd=true}).ToList();
                    holidays.AddRange(list);
                }
                Response.Data.Holidays = holidays;
                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
