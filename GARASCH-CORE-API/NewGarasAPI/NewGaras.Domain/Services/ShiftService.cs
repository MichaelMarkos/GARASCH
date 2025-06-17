using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Shift;
using NewGaras.Infrastructure.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;

namespace NewGaras.Domain.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ShiftService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public BaseResponseWithId AddListOfShifts(AddListOfShiftsDto dto, long creator)
        {
            BaseResponseWithId response = new BaseResponseWithId();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (dto.shiftDtos == null || dto.shiftDtos.Count == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "you sent Invalid data";
                    response.Errors.Add(err);
                    return response;
                }
                if (CheckShiftListOverlapping(dto.shiftDtos) == false)
                {
                    //first case We apply those shifts for this Branch Only "" (branch Id is not NULL AND ApplyShiftsForAll is NULL) ""
                    if (dto.shiftDtos.All(a => a.BranchId != null) && dto.ApplyShiftsForAll == null)
                    {
                        foreach (var shiftDto in dto.shiftDtos)
                        {
                            if (CheckShiftOverlapping(shiftDto))
                            {
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E101";
                                err.ErrorMSG = "Shift Time in " + (dto.shiftDtos.IndexOf(shiftDto) + 1) + " is overlapping with another shift in the same day";
                                response.Errors.Add(err);
                                return response;
                            }

                        }
                        var acceptedFirstShifts = _mapper.Map<List<BranchSchedule>>(dto.shiftDtos);
                        acceptedFirstShifts = acceptedFirstShifts.Select(a => { a.CreatedBy = creator; a.ModifiedBy = creator; a.CreationDate = DateTime.Now; a.ModifiedDate = DateTime.Now; return a; }).ToList();
                        _unitOfWork.BranchSchedules.AddRange(acceptedFirstShifts);
                        _unitOfWork.Complete();
                    }
                    //Second case We apply those shifts the branches that doesn't have shifts Only "" (branch Id is NULL  AND ApplyShiftsForAll is false) ""
                    else if (dto.shiftDtos.All(a => a.BranchId == null) && dto.ApplyShiftsForAll == false)
                    {
                        var Remainbranches = _unitOfWork.Branches.FindAll(a => a.BranchSchedules.Count == 0 || a.BranchSchedules == null, includes: new[] { "BranchSchedules" }).ToList();
                        foreach (var branch in Remainbranches)
                        {
                            dto.shiftDtos = dto.shiftDtos.Select(a => { a.BranchId = branch.Id; return a; }).ToList();
                            var acceptedFirstShifts = _mapper.Map<List<BranchSchedule>>(dto.shiftDtos);
                            acceptedFirstShifts = acceptedFirstShifts.Select(a => { a.CreatedBy = creator; a.ModifiedBy = creator; a.CreationDate = DateTime.Now; a.ModifiedDate = DateTime.Now; return a; }).ToList();
                            _unitOfWork.BranchSchedules.AddRange(acceptedFirstShifts);
                            _unitOfWork.Complete();
                        }

                    }
                    //Third case We apply those shifts for All Branches"" (branch Id is NUll AND ApplyShiftsForAll is true) ""
                    /*else if (dto.shiftDtos.All(a => a.BranchId == null) && dto.ApplyShiftsForAll == true)
                    {
                        var branches = _unitOfWork.Branches.FindAll(a => true, includes: new[] { "BranchSchedules" }).ToList();
                        foreach(var branch in branches)
                        {
                            _unitOfWork.BranchSchedules.DeleteRange(branch.BranchSchedules);
                            _unitOfWork.Complete();
                            dto.shiftDtos = dto.shiftDtos.Select(a => { a.BranchId = branch.Id; return a; }).ToList();
                            var acceptedFirstShifts = _mapper.Map<List<BranchSchedule>>(dto.shiftDtos);
                            acceptedFirstShifts = acceptedFirstShifts.Select(a => { a.CreatedBy = creator; a.ModifiedBy = creator; a.CreationDate = DateTime.Now; a.ModifiedDate = DateTime.Now; return a; }).ToList();
                            _unitOfWork.BranchSchedules.AddRange(acceptedFirstShifts);
                            _unitOfWork.Complete();
                        }
                    }*/
                }
                else
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "you sent two shifts Overlapped With each Other";
                    response.Errors.Add(err);
                    return response;
                }
                /*var acceptedShifts = _mapper.Map<List<BranchSchedule>>(dto.shiftDtos);
                acceptedShifts = acceptedShifts.Select(a=> { a.CreatedBy = creator; a.ModifiedBy = creator;a.CreationDate = DateTime.Now; a.ModifiedDate = DateTime.Now;return a;}).ToList();
                _unitOfWork.BranchSchedules.AddRange(acceptedShifts);
                _unitOfWork.Complete();*/
                //response.Data = acceptedShifts.ToList();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> AddShift(AddShiftDto shiftDto, long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                /*var allShiftsOfDay = _unitOfWork.Shifts.FindAll(x => x.WeekDayId == shiftDto.WeekDayId && ).ToList();
                foreach (var item in allShiftsOfDay)
                {
                    if ((shiftDto.From > item.From && shiftDto.From < item.To) || (shiftDto.To > item.From && shiftDto.To < item.To))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.code = "E-1";
                        err.message = "Shift Time is overlapping with another shift";
                        response.Errors.Add(err);
                        return response;
                    }

                }*/
                if (CheckShiftOverlapping(shiftDto) == false)
                {

                    if (creator == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "Please Login First";
                        response.Errors.Add(error);
                        return response;
                    }
                    var weekday = _unitOfWork.WeekDays.GetById((int)shiftDto.WeekDayId);
                    if (weekday == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err104";
                        error.ErrorMSG = "Week Day Not found";
                        response.Errors.Add(error);
                        return response;
                    }

                    var shift = _mapper.Map<BranchSchedule>(shiftDto);
                    shift.CreatedBy = creator;
                    shift.ModifiedBy = creator;
                    shift.CreationDate = DateTime.Now;
                    shift.ModifiedDate = DateTime.Now;
                    var AddedShift = _unitOfWork.BranchSchedules.Add(shift);
                    _unitOfWork.Complete();
                    response.ID = AddedShift.Id;
                    return response;
                }
                else
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "shift can not not be added in the same time of another shift";
                    response.Errors.Add(error);
                    return response;
                }
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



        public bool CheckShiftListOverlapping(List<AddShiftDto> shiftDtos)
        {
            var allShiftsOfDay = shiftDtos.ToArray();
            for (int i = 1; i < allShiftsOfDay.Length; i++)
            {
                if ((allShiftsOfDay[i - 1].WeekDayId == allShiftsOfDay[i].WeekDayId) && ((allShiftsOfDay[i - 1].From > allShiftsOfDay[i].From && allShiftsOfDay[i - 1].From < allShiftsOfDay[i].To) || (allShiftsOfDay[i - 1].To > allShiftsOfDay[i].From && allShiftsOfDay[i - 1].To < allShiftsOfDay[i].To) || (allShiftsOfDay[i - 1].From == allShiftsOfDay[i].From && allShiftsOfDay[i - 1].To == allShiftsOfDay[i].To) && !(allShiftsOfDay[i - 1].From == allShiftsOfDay[i].To)))
                {
                    return true;
                }
            }
            return false;
        }
        public bool CheckShiftOverlapping(AddShiftDto shiftDto)
        {
            var allShiftsOfDay = _unitOfWork.BranchSchedules.FindAll(x => x.WeekDayId == shiftDto.WeekDayId && x.BranchId == shiftDto.BranchId).ToList();
            foreach (var item in allShiftsOfDay)
            {
                if ((shiftDto.From > item.From && shiftDto.From < item.To) || (shiftDto.To > item.From && shiftDto.To < item.To) || (shiftDto.From == item.From && shiftDto.To == item.To) && !(shiftDto.From == item.To))
                {
                    return true;
                }
            }
            return false;
        }

        public BaseResponseWithData<BranchScheduleDto> GetShift(long shiftId)
        {
            BaseResponseWithData<BranchScheduleDto> response = new BaseResponseWithData<BranchScheduleDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var shift = _unitOfWork.BranchSchedules.GetById(shiftId);
                if (shift != null)
                {
                    var finalShift = _mapper.Map<BranchScheduleDto>(shift);
                    response.Data = finalShift;
                }
                else
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "shift not found";
                    response.Errors.Add(error);
                    return response;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "Err10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithData<List<GetBranchScheduls>>> GetShifts([FromHeader] int branchId)
        {
            var response = new BaseResponseWithData<List<GetBranchScheduls>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var shifts = new List<GetBranchScheduls>();
                var allShiftsOfDay = await _unitOfWork.BranchSchedules.FindAllAsync(a => a.BranchId == branchId);
                var shiftList = allShiftsOfDay.GroupBy(a => a.ShiftNumber).ToList();
                shifts = shiftList.Select(a => new GetBranchScheduls() { shiftNumber = a.Key, shifts = _mapper.Map<List<BranchScheduleDto>>(a) }).ToList();
                response.Data = shifts;
                _unitOfWork.Complete();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "Err10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> UpdateShift(List<AddShiftDto> shiftDtos, long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                foreach (var shiftDto in shiftDtos)
                {
                    if (shiftDto.Id != null)
                    {

                        var shift = _unitOfWork.BranchSchedules.GetById((long)shiftDto.Id);
                        if (shift != null)
                        {
                            if (shiftDto.Active == false)
                            {
                                var workinghours = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.ShiftId == shift.Id).ToList();
                                if (workinghours.Count > 0)
                                {
                                    for (var i = 0; i < workinghours.Count; i++)
                                    {
                                        var workinghour = workinghours[i];
                                        workinghour.ShiftId = null;
                                        _unitOfWork.Complete();
                                    }
                                }
                                _unitOfWork.BranchSchedules.Delete(shift);
                                _unitOfWork.Complete();
                            }
                            else
                            {

                                var allShiftsOfDay = _unitOfWork.BranchSchedules.FindAll(x => x.WeekDayId == shiftDto.WeekDayId && x.Id != shiftDto.Id && x.BranchId == shiftDto.BranchId, includes: new[] { "WorkingHourseTrackings" }).ToList();
                                foreach (var item in allShiftsOfDay)
                                {
                                    if ((shiftDto.From > item.From && shiftDto.From < item.To) || (shiftDto.To > item.From && shiftDto.To < item.To) ||
                                        (shiftDto.From == item.From && shiftDto.To == item.To) ||
                                        (item.From > shiftDto.From && item.From < shiftDto.To) || (item.To > shiftDto.From && item.To < shiftDto.To))
                                    {
                                        response.Result = false;
                                        Error err = new Error();
                                        err.ErrorCode = "Err101";
                                        err.ErrorMSG = "Shift Time is overlapping with another shift";
                                        response.Errors.Add(err);
                                        return response;
                                    }

                                }
                                /*var checkdelete = _unitOfWork.BranchSchedules.FindAll(a => a.Id == shiftDto.Id).FirstOrDefault();
                                if (checkdelete != null)
                                {
                                    if (shiftDto.Active == false)
                                    {
                                        _unitOfWork.BranchSchedules.Delete(checkdelete);
                                        _unitOfWork.Complete();
                                    }
                                }*/
                                if (creator == 0)
                                {
                                    response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err103";
                                    error.ErrorMSG = "Please Login First";
                                    response.Errors.Add(error);
                                    return response;
                                }
                                var weekday = _unitOfWork.WeekDays.GetById((int)shiftDto.WeekDayId);
                                if (weekday == null)
                                {
                                    response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err104";
                                    error.ErrorMSG = "Week Day Not found";
                                    response.Errors.Add(error);
                                    return response;
                                }

                                _mapper.Map<AddShiftDto, BranchSchedule>(shiftDto, shift);
                                shift.ModifiedDate = DateTime.Now;
                                shift.ModifiedBy = creator;
                                var EditedShift = _unitOfWork.BranchSchedules.Update(shift);
                                _unitOfWork.Complete();
                                response.ID = EditedShift.Id;
                            }
                        }

                    }
                    else
                    {
                        AddShift(shiftDto, creator);
                    }
                }
                return response;

            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "Err10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }






        public Tuple<int, int> CalculateNOBranchWorkingDay(int BranchId)
        {
            int totalDays = 0;
            int WeekEndDays = 0;
            var datenow = DateTime.Now;
            var BranchSettingdb = _unitOfWork.BranchSetting.Find(x => x.BranchId == BranchId);
            if (BranchSettingdb != null && BranchSettingdb.PayrollFrom != null && BranchSettingdb.PayrollTo != null)
            {
                var excludedDays = _unitOfWork.BranchSchedules.FindAll((x => x.BranchId == BranchId && x.WeekDay.IsWeekEnd == false), new[] { "WeekDay" })
                    .Select(x => Enum.Parse<DayOfWeek>(x.WeekDay?.Day)).Distinct().ToList();
                // Parse the day name to the DayOfWeek enum
                // HashSet<DayOfWeek> excludedDays = new HashSet<DayOfWeek>();
                //if (Enum.TryParse<DayOfWeek>(dayName, true, out DayOfWeek dayOfWeek))
                //{
                //    excludedDays.Add(dayOfWeek);
                //}

                DateOnly start = new DateOnly(datenow.Year, datenow.Month, (int)BranchSettingdb.PayrollFrom).AddMonths(-1); // default in last month
                DateOnly end = new DateOnly(datenow.Year, datenow.Month, (int)BranchSettingdb.PayrollTo);
                int diffrence = (int)BranchSettingdb.PayrollTo - (int)BranchSettingdb.PayrollFrom;
                if (diffrence >= 15) // the same month
                {
                    start = new DateOnly(datenow.Year, datenow.Month, (int)BranchSettingdb.PayrollFrom); // in the same month
                }
                for (DateOnly date = start; date < end; date = date.AddDays(1))
                {
                    if (excludedDays.Contains(date.DayOfWeek))
                    {
                        totalDays++;
                    }
                    else
                    {
                        WeekEndDays++;
                    }
                }
            }
            return Tuple.Create(totalDays, WeekEndDays);
        }

    }
}
