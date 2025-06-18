using AutoMapper;
using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Medical.DoctorSchedule;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Medical;
using NewGaras.Infrastructure.Models.Medical.Filters;
using NewGaras.Infrastructure.Models.Medical.UsedInResponse;
using NewGarasAPI.Models.User;
using Org.BouncyCastle.Bcpg;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NewGaras.Domain.Services.Medical
{
    public class DoctorScheduleService : IDoctorScheduleService
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
        public DoctorScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public BaseResponseWithId<long> AddDoctorSchedulestatus(string DoctorScheduleStatusType)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var newStatusType = new MedicalDoctorScheduleStatus()
                {
                    DoctorScheduleStatusType = DoctorScheduleStatusType
                };

                _unitOfWork.MedicalDoctorScheduleStatus.Add(newStatusType);
                _unitOfWork.Complete();

                response.ID = newStatusType.Id;
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

        public SelectDDLResponse GetDoctorSchedulestatus()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.MedicalDoctorScheduleStatus.GetAll().ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.DoctorScheduleStatusType;

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
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

        public BaseResponseWithId<long> AddDoctorSchedule(DoctorScheduleDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region validation
            var doctor = _unitOfWork.HrUsers.GetById(dto.DoctorID);
            if (doctor == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Doctor with this ID";
                response.Errors.Add(err);
                return response;
            }

            var status = _unitOfWork.MedicalDoctorScheduleStatus.GetById(dto.StatusID);
            if (status == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No status with this ID";
                response.Errors.Add(err);
                return response;
            }

            var percentage  = _unitOfWork.MedicalDoctorPercentageTypes.GetById(dto.PercentageTypeID);
            if (percentage == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Percentage with this ID";
                response.Errors.Add(err);
                return response;
            }

            var team = _unitOfWork.Teams.GetById(dto.TeamID);
            if (team == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Team with this ID";
                response.Errors.Add(err);
                return response;
            }

            var weekDay = _unitOfWork.WeekDays.GetById(dto.WeekDayID);
            if (weekDay == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Week Day with this ID";
                response.Errors.Add(err);
                return response;
            }

            var room = _unitOfWork.DoctorRooms.GetById(dto.RoomId);
            if (room == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Room with this ID";
                response.Errors.Add(err);
                return response;
            }

            if(dto.StartDate.Date < DateTime.Now)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "you can not make Doctor Schedule with start date already passed";
                response.Errors.Add(err);
                return response;
            }

            if(dto.BranchID == null)
            {
                var branchDB = _unitOfWork.Branches.GetById(dto.BranchID??0);
                if(branchDB == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No branch eith this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion

            #region doc. old schedules 
            var docScheduleList = _unitOfWork.DoctorSchedules.FindAll(a => a.DoctorId == dto.DoctorID);
            if (docScheduleList != null) 
            {
                foreach (var sch in docScheduleList)
                {
                    if(sch.StartDate.Date <= dto.StartDate.Date && (dto.StartDate.Date <= sch.EndDate || sch.EndDate == null))
                    {
                        if(dto.IntervalFrom.IsBetween(sch.IntervalFrom,sch.IntervalTo))
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E101";
                            err.ErrorMSG = "you can not make Doctor Schedule in interval thats have already a Schedule for the same doctor in the same time";
                            response.Errors.Add(err);
                            return response;
                        }
                        if (dto.IntervalTo.IsBetween(sch.IntervalFrom, sch.IntervalTo))
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E101";
                            err.ErrorMSG = "you can not make Doctor Schedule in interval thats have already a Schedule for the same doctor in the same time";
                            response.Errors.Add(err);
                            return response;
                        }
                        
                    }
                }
            }
            #endregion


            #region check if the room is already reserved
            var oldSchedules = _unitOfWork.DoctorSchedules.FindAll(a => a.RoomId ==  dto.RoomId);
            if (oldSchedules != null)
            {
                foreach (var sch in oldSchedules)
                {
                    if (sch.StartDate.Date <= dto.StartDate.Date && (dto.StartDate.Date <= sch.EndDate || sch.EndDate == null))
                    {
                        if (dto.IntervalFrom.IsBetween(sch.IntervalFrom, sch.IntervalTo) && sch.WeekDayId == dto.WeekDayID )
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E101";
                            err.ErrorMSG = "you can not make Doctor Schedule in this room in this interval (Room is reserved in this interval)";
                            response.Errors.Add(err);
                            return response;
                        }
                        if (dto.IntervalTo.IsBetween(sch.IntervalFrom, sch.IntervalTo) && sch.WeekDayId == dto.WeekDayID)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E101";
                            err.ErrorMSG = "you can not make Doctor Schedule in this room in this interval (Room is reserved in this interval)";
                            response.Errors.Add(err);
                            return response;
                        }
                        bool isOverlap = dto.IntervalFrom < sch.IntervalFrom && dto.IntervalTo > sch.IntervalFrom;
                        if (isOverlap)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E101";
                            err.ErrorMSG = "you can not make Doctor Schedule in this room in this interval (Room is reserved in this interval) interval overlap";
                            response.Errors.Add(err);
                            return response;
                        }
                    }
                }
            }
            #endregion

            #region check if Doc. in this team (Speciality) or not
            var DoctorTeam = _unitOfWork.UserTeams.FindAll(a => a.UserId == doctor.UserId);
            if(DoctorTeam == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "This Doctor (user) has no team";
                response.Errors.Add(err);
                return response;
            }
            var doctorTeamsIDList = DoctorTeam.Select(a => a.TeamId).ToList();
            if (!doctorTeamsIDList.Contains(dto.TeamID))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "This Doctor (user) is not in this team";
                response.Errors.Add(err);
                return response;
            }
            #endregion
            try
            {
                if(dto.Period == null)
                {
                    var docSchedule = _mapper.Map<DoctorSchedule>(dto);
                    docSchedule.CreationDate = DateTime.Now;
                    docSchedule.CreatedBy = validation.userID;
                    docSchedule.ModifiedBy = validation.userID;
                    docSchedule.ModificationDate = DateTime.Now;
                    //if (dto.HoldQuantity == null) docSchedule.HoldQuantity = 5;         //default value if user do not sent it

                    _unitOfWork.DoctorSchedules.Add(docSchedule);
                    _unitOfWork.Complete();

                    response.ID = docSchedule.Id;
                }
                else        //to divide the period from (intervalFrom) to (intervalTo) into multi. schedules by sparated by the period given
                {
                    var loopTime = dto.IntervalFrom;
                    var newDocSchedulesList = new List<DoctorSchedule>();
                    while (loopTime < dto.IntervalTo)
                    {
                        var CurrentSchedule = _mapper.Map<DoctorSchedule>(dto);
                        CurrentSchedule.CreationDate = DateTime.Now;
                        CurrentSchedule.CreatedBy = validation.userID;
                        CurrentSchedule.ModifiedBy = validation.userID;
                        CurrentSchedule.ModificationDate = DateTime.Now;
                        //if (dto.HoldQuantity == null) CurrentSchedule.HoldQuantity = 5;         //default value if user do not sent it

                        CurrentSchedule.IntervalFrom = loopTime;
                        CurrentSchedule.IntervalTo = loopTime.AddMinutes(dto.Period??0);        //add the period to the intervalTo 

                        newDocSchedulesList.Add(CurrentSchedule);
                        loopTime = loopTime.AddMinutes(dto.Period??10);
                    }
                    _unitOfWork.DoctorSchedules.AddRange(newDocSchedulesList);
                    _unitOfWork.Complete();
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

        public BaseResponseWithId<long> EditDoctorSchedule(EditDoctorScheduleDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region validation
            if(dto.Id == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "Doctor Schedule ID is mandatory";
                response.Errors.Add(err);
                return response;
            }

            var currentSchedule = _unitOfWork.DoctorSchedules.GetById(dto.Id);
            if(currentSchedule == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Schedule with this ID";
                response.Errors.Add(err);
                return response;
            }
            //if(dto.DoctorID != null)
            //{
            //    var doctor = _unitOfWork.HrUsers.GetById(dto.DoctorID??0);
            //    if (doctor == null)
            //    {
            //        response.Result = false;
            //        Error err = new Error();
            //        err.ErrorCode = "E101";
            //        err.ErrorMSG = "No Doctor with this ID";
            //        response.Errors.Add(err);
            //        return response;
            //    }
            //}


            if (dto.StatusID != null)
            {
                var status = _unitOfWork.MedicalDoctorScheduleStatus.GetById(dto.StatusID ?? 0);
                if (status == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No status with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if(dto.PercentageTypeID != null)
            {
                var percentage = _unitOfWork.MedicalDoctorPercentageTypes.GetById(dto.PercentageTypeID ?? 0);
                if (percentage == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Percentage with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }
           
            //if(dto.SpecialityId != null)
            //{
            //    var team = _unitOfWork.Teams.GetById(dto.SpecialityId ?? 0);
            //    if (team == null)
            //    {
            //        response.Result = false;
            //        Error err = new Error();
            //        err.ErrorCode = "E101";
            //        err.ErrorMSG = "No Speciality with this ID";
            //        response.Errors.Add(err);
            //        return response;
            //    }
            //}
            
            if(dto.WeekDayID != null)
            {
                var weekDay = _unitOfWork.WeekDays.GetById(dto.WeekDayID ?? 0);
                if (weekDay == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Week Day with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            
            if (dto.RoomId != null)
            {
                var room = _unitOfWork.DoctorRooms.GetById(dto.RoomId ?? 0); 
                if (room == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Room with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion

            #region check if the room is already reserved
            var oldSchedules = _unitOfWork.DoctorSchedules.FindAll(a => a.RoomId == dto.RoomId);
            if (oldSchedules != null)
            {
                foreach (var sch in oldSchedules)
                {
                    if(sch.Id != dto.Id)
                    {
                        bool isIntersecting = (sch.StartDate.Date < dto.EndDate?.Date && (sch.EndDate?.Date > currentSchedule.StartDate.Date));
                        if (isIntersecting)
                        {
                            if (dto.IntervalFrom?.IsBetween(sch.IntervalFrom, sch.IntervalTo)??false && sch.WeekDayId == dto.WeekDayID)
                            {
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E101";
                                err.ErrorMSG = "you can not make Doctor Schedule in this room in this interval (Room is reserved in this interval)";
                                response.Errors.Add(err);
                                return response;
                            }
                            if (dto.IntervalTo?.AddMinutes(-1).IsBetween(sch.IntervalFrom, sch.IntervalTo)??false && sch.WeekDayId == dto.WeekDayID)
                            {
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E101";
                                err.ErrorMSG = "you can not make Doctor Schedule in interval thats have already a Schedule for the same doctor in the same time";
                                response.Errors.Add(err);
                                return response;
                            }
                            bool isOverlap = dto.IntervalFrom < sch.IntervalFrom && dto.IntervalTo > sch.IntervalFrom;
                            if (isOverlap)
                            {
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E101";
                                err.ErrorMSG = "you can not make Doctor Schedule in this room in this interval (Room is reserved in this interval) interval overlap";
                                response.Errors.Add(err);
                                return response;
                            }

                        }
                    }
                }
            }
            #endregion
            try
            {
                var docSchedule = _unitOfWork.DoctorSchedules.GetById(dto.Id);
                if(docSchedule == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Doctor Schedule with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                //if (dto.DoctorID != null) docSchedule.DoctorId = dto.DoctorID??0;
                if (dto.Capacity != null) docSchedule.Capacity = dto.Capacity??0;
                if (dto.IntervalFrom != null) docSchedule.IntervalFrom = dto.IntervalFrom?? TimeOnly.FromDateTime(DateTime.Now);
                if (dto.IntervalTo != null) docSchedule.IntervalTo = dto.IntervalTo ?? TimeOnly.FromDateTime(DateTime.Now);
                if (dto.ConsultationPrice != null) docSchedule.ConsultationPrice = dto.ConsultationPrice ?? 0;
                if (dto.StatusID != null) docSchedule.StatusId = dto.StatusID ?? 0;
                //if (dto.StartDate != null) docSchedule.StartDate = dto.StartDate ?? DateTime.Now;
                if (dto.EndDate != null) docSchedule.EndDate = dto.EndDate ?? DateTime.Now;
                if (dto.RoomId != null) docSchedule.RoomId = dto.RoomId??0;
                if (dto.PercentageTypeID != null) docSchedule.PercentageTypeId = dto.PercentageTypeID ?? 0;
                if (dto.ExaminationPrice != null) docSchedule.ExaminationPrice = dto.ExaminationPrice ?? 0;
                //if (dto.SpecialityId != null) docSchedule.DoctorSpecialityId = dto.SpecialityId ?? 0;
                if (dto.WeekDayID != null) docSchedule.WeekDayId = dto.WeekDayID ?? 0;
                //if (dto.HoldQuantity != null) docSchedule.HoldQuantity = dto.HoldQuantity;

                docSchedule.ModifiedBy = validation.userID;
                docSchedule.ModificationDate = DateTime.Now;

                
                _unitOfWork.Complete();

                response.ID = docSchedule.Id;
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

        public BaseResponseWithDataAndHeader<GetDoctorScheduleList> GetDoctorScheduleList(GetDoctorScheduleListFilters filters)
        {
            var response = new BaseResponseWithDataAndHeader<GetDoctorScheduleList>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region date validation
            var startDate = DateTime.Now;
            if (!string.IsNullOrEmpty(filters.DateFrom)) 
            {
                if(!DateTime.TryParse(filters.DateFrom, out startDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Data From format";
                    response.Errors.Add(err);
                    return response;
                }
            }

            var EndDate = DateTime.Now;
            if (!string.IsNullOrEmpty(filters.DateTo))
            {
                if (!DateTime.TryParse(filters.DateTo, out EndDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Data To format";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if( (!string.IsNullOrEmpty(filters.DateFrom)) && (!string.IsNullOrEmpty(filters.DateTo)) && (!string.IsNullOrEmpty(filters.DayDate)))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "can not filter by Interval (from - to) and by Day Date at the same Time";
                response.Errors.Add(err);
                return response;
            }

            var DayDate = DateTime.Now;
            if (!string.IsNullOrEmpty(filters.DayDate))
            {
                if (!DateTime.TryParse(filters.DayDate, out DayDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Day Date format";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion

            try
            {
                var doctorScheduleListQueryable = _unitOfWork.DoctorSchedules.FindAllQueryable(a => true, new[] { "Doctor", "WeekDay" , "Status", "PercentageType", "DoctorSpeciality", "Room", "Branch" });

                if(filters.DoctorID != null)
                {
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.DoctorId  == filters.DoctorID).AsQueryable();
                }

                if (filters.SpecialtyID != null)
                {
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.DoctorSpecialityId == filters.SpecialtyID).AsQueryable();                
                }

                if (filters.BranchID != null)
                {
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.BranchId == filters.BranchID).AsQueryable();
                }

                if (!string.IsNullOrEmpty(filters.DateFrom)) 
                {
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.StartDate.Date <= startDate.Date).AsQueryable();
                }

                if (!string.IsNullOrEmpty(filters.DateTo)) //need to be tested (end date)
                {
                    var EndWeekData = EndDate.AddDays(-7);
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.StartDate.Date <= startDate.Date && (a.EndDate >= EndDate || a.EndDate == null || a.EndDate >= EndDate.AddDays(-7))).AsQueryable();
                }

                if ((string.IsNullOrEmpty(filters.DateFrom)) && (string.IsNullOrEmpty(filters.DateTo)) && (!string.IsNullOrEmpty(filters.DayDate)))
                {
                    var dayName = DayDate.ToString("dddd");
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.StartDate.Date <= DayDate.Date && (a.EndDate >= DayDate || a.EndDate == null)).AsQueryable();
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.WeekDay.Day.ToLower() == dayName.ToLower()).AsQueryable();
                }

                var doctorSchedulePagedList = PagedList<DoctorSchedule>.Create(doctorScheduleListQueryable, filters.currentPage, filters.numberOfItemsPerPage);

                var doctorScheduleList = new GetDoctorScheduleList();
                var doctorSchedule = new List<GetDoctorScheduleDTO>();

                foreach (var schedule in doctorSchedulePagedList)
                {
                    var newDoctorSchedule = new GetDoctorScheduleDTO();

                    newDoctorSchedule.ID = schedule.Id;
                    newDoctorSchedule.DoctorID = schedule.DoctorId;
                    newDoctorSchedule.DoctorName = schedule.Doctor.FirstName + " " + schedule.Doctor.LastName;
                    newDoctorSchedule.Capacity = schedule.Capacity;
                    newDoctorSchedule.IntervalFrom = schedule.IntervalFrom;
                    newDoctorSchedule.IntervalTo = schedule.IntervalTo;
                    newDoctorSchedule.ConsultationPrice = schedule.ConsultationPrice;
                    newDoctorSchedule.StatusID = schedule.StatusId;
                    newDoctorSchedule.StatusName = schedule.Status.DoctorScheduleStatusType;
                    newDoctorSchedule.RoomId = schedule.RoomId;
                    newDoctorSchedule.RoomName = schedule.Room.Name;
                    newDoctorSchedule.PercentageTypeID = schedule.PercentageTypeId;
                    newDoctorSchedule.PercentageTypeName = schedule.PercentageType.DoctorPercentageType;
                    newDoctorSchedule.ExaminationPrice = schedule.ExaminationPrice;
                    newDoctorSchedule.SpecialityID = schedule.DoctorSpecialityId;
                    newDoctorSchedule.SpecialityName = schedule.DoctorSpeciality.Name;
                    newDoctorSchedule.WeekDayID = schedule.WeekDayId;
                    newDoctorSchedule.WeekDayName = schedule.WeekDay.Day;
                    newDoctorSchedule.StartDate = schedule.StartDate.ToString("yyyy-MM-dd");
                    newDoctorSchedule.EndDate = schedule.EndDate?.ToString("yyyy-MM-dd"); 
                    //newDoctorSchedule.HoldQuantity = schedule.HoldQuantity??0;
                    newDoctorSchedule.BranchID = schedule.BranchId;
                    newDoctorSchedule.BranchName = schedule.Branch?.Name;

                    doctorSchedule.Add(newDoctorSchedule);
                }

                doctorScheduleList.DoctorScheduleDTO = doctorSchedule;
                response.Data = doctorScheduleList;

                var PaginationHeader = new PaginationHeader()
                {
                    CurrentPage = filters.currentPage,
                    ItemsPerPage = filters.numberOfItemsPerPage,
                    TotalItems = doctorSchedulePagedList.TotalCount,
                    TotalPages = doctorSchedulePagedList.TotalPages,
                };

                response.PaginationHeader = PaginationHeader;
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

        public BaseResponseWithId<long> AddExaminationAndExaminationPrices(AddExaminationAndExaminationPricesDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region validation
            if (dto.DoctorID != null)
            {
                var doctor = _unitOfWork.HrUsers.GetById(dto.DoctorID ?? 0);
                if (doctor == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Doctor with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

           

            if (dto.SpecialityID != null)
            {
                var doctor = _unitOfWork.Teams.GetById(dto.SpecialityID ?? 0);
                if (doctor == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Speciality with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if( dto.SpecialityID != null && dto.DoctorID != null) 
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "Enter only one from Doctor ID or Speciality ID ";
                response.Errors.Add(err);
                return response;
            }
            #endregion


            try
            {
                if(dto.DoctorID != null)
                {
                    var doctorSchedulesList = _unitOfWork.DoctorSchedules.FindAll(a => a.DoctorId == dto.DoctorID);

                    foreach (var doc in doctorSchedulesList)
                    {
                        doc.ExaminationPrice = dto.ExaminationPrice;
                        doc.ConsultationPrice = dto.consultationPrice;
                    }

                    _unitOfWork.Complete();
                }

                if (dto.SpecialityID != null)
                {
                    var doctorSchedulesList = _unitOfWork.DoctorSchedules.FindAll(a => a.DoctorSpecialityId == dto.SpecialityID);

                    foreach (var doc in doctorSchedulesList)
                    {
                        doc.ExaminationPrice = dto.ExaminationPrice;
                        doc.ConsultationPrice = dto.consultationPrice;
                    }

                    _unitOfWork.Complete();
                }


                
                //if (dto.DepartmentID != null)
                //{
                //    var deptList  = _unitOfWork.Departments.GetById(dto.DepartmentID ?? 0);

                //    var teamsList = _unitOfWork.Teams.FindAll(a => a.DepartmentId  == dto.DepartmentID);

                //    var teamsIDs = teamsList.Select(a => a.Id).ToList();

                //    var doctorSchedulesList = _unitOfWork.DoctorSchedules.FindAll(a => teamsIDs.Contains(a.DoctorSpecialityId));

                //    foreach (var doc in doctorSchedulesList)
                //    {
                //        doc.ExaminationPrice = dto.ExaminationPrice;
                //        doc.ConsultationPrice = dto.consultationPrice;
                //    }

                //    _unitOfWork.Complete();
                //}

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

        public BaseResponseWithData<List<long?>> GetRoomsList()
        {
            var response = new BaseResponseWithData<List<long?>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var docSchedQueryable = _unitOfWork.DoctorSchedules.FindAllQueryable(a => true);
                var roomsList = docSchedQueryable.Select(a => a.RoomId).Distinct().ToList();

                response.Data = roomsList;
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

        public BaseResponseWithData<GetDoctorScheduleGroupByDocNameList> GetDoctorScheduleListGroupByDoctorName(GetDoctorScheduleListGroupByDoctorNameFilters filters)
        {
            var response = new BaseResponseWithData<GetDoctorScheduleGroupByDocNameList>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region date validation
            if (string.IsNullOrEmpty(filters.DayDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "please enter Day Date!";
                response.Errors.Add(err);
                return response;
            }

            var filterDate = DateTime.Now;
            
            if (!DateTime.TryParse(filters.DayDate, out filterDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Invalid Day Date format";
                response.Errors.Add(err);
                return response;
            }

            #endregion

            #region sort Validation
            if (filters.SortByDoctornameAsce == true && filters.SortByDoctornameDesc == true)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "can not sort Ascending and Descending orders at the same time , choose only one";
                response.Errors.Add(err);
                return response;
            }
            #endregion
            try
            {
                var doctorScheduleListQueryable = _unitOfWork.DoctorSchedules.FindAllQueryable(a => true, new[] { "Doctor", "WeekDay", "Status", "PercentageType", "DoctorSpeciality", "Room", "Branch" });

                if (filters.DoctorID != null)
                {
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.DoctorId == filters.DoctorID).AsQueryable();
                }
                if (filters.SpecialtyID != null)
                {
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.DoctorSpecialityId == filters.SpecialtyID).AsQueryable();
                }
                if (filters.BranchID != null)
                {
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.BranchId == filters.BranchID).AsQueryable();
                }
                if (!string.IsNullOrEmpty(filters.searchKey))
                {
                    var searchKeyValue =  HttpUtility.UrlDecode(filters.searchKey);
                    doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => (a.Doctor.FirstName + a.Doctor.MiddleName + a.Doctor.LastName).Contains(searchKeyValue.Replace(" ", "")) ).AsQueryable();
                }
                
                doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.StartDate.Date <= filterDate.Date && (a.EndDate >= filterDate || a.EndDate == null)).AsQueryable();

                var dayName = filterDate.ToString("dddd");
                doctorScheduleListQueryable = doctorScheduleListQueryable.Where(a => a.WeekDay.Day.ToLower() == dayName.ToLower()).AsQueryable();
                
                if(filters.SortByDoctornameAsce == true)
                {
                    doctorScheduleListQueryable = doctorScheduleListQueryable.OrderBy(a => a.Doctor.FirstName + (a.Doctor.MiddleName ?? "") + a.Doctor.LastName).AsQueryable();
                }

                if (filters.SortByDoctornameDesc == true)
                {
                    doctorScheduleListQueryable = doctorScheduleListQueryable.OrderByDescending(a => a.Doctor.FirstName + (a.Doctor.MiddleName ?? "") + a.Doctor.LastName).AsQueryable();
                }

                var docScheduleList = doctorScheduleListQueryable.ToList();
                var docScheduleListGroupedByDoc = docScheduleList.GroupBy(a => a.Doctor).ToList();

                var finalData = new GetDoctorScheduleGroupByDocNameList();
                var DoctorsList = new List<GetDoctorScheduleGroupByDocName>();
                foreach (var doc in docScheduleListGroupedByDoc)
                {
                    var currentDoctor = new GetDoctorScheduleGroupByDocName();

                    currentDoctor.DoctorName = doc.Key.FirstName +" " + doc.Key?.MiddleName + " " + doc.Key.LastName;
                    currentDoctor.DoctorID = doc.Key.Id;

                    var currentScheduleList = new List<DoctorScheduleDTOForGrouping>();
                    foreach (var schedule in doc)
                    {
                        var newDoctorSchedule = new DoctorScheduleDTOForGrouping();

                        newDoctorSchedule.ID = schedule.Id;
                        newDoctorSchedule.DoctorID = schedule.DoctorId;
                        newDoctorSchedule.DoctorName = schedule.Doctor.FirstName + " " + schedule.Doctor.LastName;
                        newDoctorSchedule.Capacity = schedule.Capacity;
                        newDoctorSchedule.IntervalFrom = schedule.IntervalFrom;
                        newDoctorSchedule.IntervalTo = schedule.IntervalTo;
                        newDoctorSchedule.ConsultationPrice = schedule.ConsultationPrice;
                        newDoctorSchedule.StatusID = schedule.StatusId;
                        newDoctorSchedule.StatusName = schedule.Status.DoctorScheduleStatusType;
                        newDoctorSchedule.RoomId = schedule.RoomId;
                        newDoctorSchedule.RoomName = schedule.Room.Name;
                        newDoctorSchedule.PercentageTypeID = schedule.PercentageTypeId;
                        newDoctorSchedule.PercentageTypeName = schedule.PercentageType.DoctorPercentageType;
                        newDoctorSchedule.ExaminationPrice = schedule.ExaminationPrice;
                        newDoctorSchedule.SpecialityID = schedule.DoctorSpecialityId;
                        newDoctorSchedule.SpecialityName = schedule.DoctorSpeciality.Name;
                        newDoctorSchedule.WeekDayID = schedule.WeekDayId;
                        newDoctorSchedule.WeekDayName = schedule.WeekDay.Day;
                        newDoctorSchedule.StartDate = schedule.StartDate.ToString("yyyy-MM-dd");
                        newDoctorSchedule.EndDate = schedule.EndDate?.ToString("yyyy-MM-dd");
                        //newDoctorSchedule.HoldQuantity = schedule.HoldQuantity??0;
                        newDoctorSchedule.BranchID = schedule.BranchId;
                        newDoctorSchedule.BranchName = schedule.Branch?.Name;

                        currentScheduleList.Add(newDoctorSchedule);
                    }
                    currentDoctor.DoctorScheduleList = currentScheduleList;

                    DoctorsList.Add(currentDoctor);
                }
                finalData.DoctorList = DoctorsList;
                response.Data = finalData;
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

        public SelectDDLResponse GetPercentageTypeListForDoctorSchedule()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.MedicalDoctorPercentageTypes.FindAll(x => true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.DoctorPercentageType;

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
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

        public SelectDDLResponse GetSpecialityListForDoctorSchedule(long? DoctorID)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListQueryable = _unitOfWork.Teams.FindAllQueryable(x => true).AsQueryable();

                    if(DoctorID != null)
                    {
                        var hruser = _unitOfWork.HrUsers.GetById(DoctorID??0);
                        if(hruser != null)
                        {
                            var DoctorTeamsList = _unitOfWork.UserTeams.FindAll(x => x.UserId == hruser.UserId).ToList();
                            var TeamsIDList = DoctorTeamsList.Select(a => a.TeamId).ToList();
                            ListQueryable = ListQueryable.Where(a => TeamsIDList.Contains(a.Id));
                        }
                    }

                    var ListDB = ListQueryable.ToList();

                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Name;

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
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

        public SelectDDLResponse GetWeekDaysListForDoctorSchedule()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.WeekDays.FindAll(x => true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Day;

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
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

        public BaseResponseWithId<long> CancelDoctorSchedule(CancelDoctorScheduleDTO data, long userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region date validation
            var startDate = DateTime.Now;
            if (!string.IsNullOrEmpty(data.IntervalFrom))
            {
                if (!DateTime.TryParse(data.IntervalFrom, out startDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Data From format";
                    response.Errors.Add(err);
                    return response;
                }
            }

            var EndDate = DateTime.Now;
            if (!string.IsNullOrEmpty(data.IntervalTo))
            {
                if (!DateTime.TryParse(data.IntervalTo, out EndDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "Invalid Data To format";
                    response.Errors.Add(err);
                    return response;
                }
            }

            var DocSchedule = _unitOfWork.DoctorSchedules.GetById(data.DoctorScheduleID);
            if (DocSchedule == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "No Doctor schedule with this ID";
                response.Errors.Add(err);
                return response;
            }

            var Doctor = _unitOfWork.HrUsers.GetById(data.DoctorID);
            if (Doctor == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "No Doctor with this ID";
                response.Errors.Add(err);
                return response;
            }
            #endregion


            try
            {
                DocSchedule.EndDate = startDate.AddDays(-1);
                DocSchedule.ModificationDate = DateTime.Now;
                DocSchedule.ModifiedBy = userID;

                var newDocSchedule = new DoctorSchedule();

                newDocSchedule.DoctorId = DocSchedule.DoctorId;
                newDocSchedule.Capacity = DocSchedule.Capacity;
                newDocSchedule.IntervalFrom = DocSchedule.IntervalFrom;
                newDocSchedule.IntervalTo = DocSchedule.IntervalTo;
                newDocSchedule.ConsultationPrice = DocSchedule.ConsultationPrice;
                newDocSchedule.StatusId = DocSchedule.StatusId;
                newDocSchedule.RoomId = DocSchedule.RoomId;
                newDocSchedule.StartDate = EndDate.AddDays(1);
                newDocSchedule.EndDate = null;
                newDocSchedule.PercentageTypeId = DocSchedule.PercentageTypeId;
                newDocSchedule.DoctorSpecialityId = DocSchedule.DoctorSpecialityId;
                newDocSchedule.ExaminationPrice = DocSchedule.ExaminationPrice;
                newDocSchedule.CreatedBy = userID;
                newDocSchedule.CreationDate =DateTime.Now;
                newDocSchedule.ModifiedBy = userID;
                newDocSchedule.ModificationDate = DateTime.Now;
                newDocSchedule.WeekDayId = DocSchedule.WeekDayId;


                var newData = _unitOfWork.DoctorSchedules.Add(newDocSchedule);
                _unitOfWork.Complete();

                response.ID = newData.Id;
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

        public BaseResponseWithData<List<GetDoctorSchedulePerWeekDTO>> GetDoctorSchedulePerWeek(GetDoctorSchedulePerWeekFilters filters)
        {
            var response = new BaseResponseWithData<List<GetDoctorSchedulePerWeekDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region date validation
            if (string.IsNullOrEmpty(filters.DateFrom))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "please enter DateFrom!";
                response.Errors.Add(err);
                return response;
            }

            var FromDate = DateTime.Now;

            if (!DateTime.TryParse(filters.DateFrom, out FromDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Invalid DateTo format";
                response.Errors.Add(err);
                return response;
            }

            if (string.IsNullOrEmpty(filters.DateTo))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "please enter DateTo!";
                response.Errors.Add(err);
                return response;
            }

            var ToDate = DateTime.Now;

            if (!DateTime.TryParse(filters.DateTo, out ToDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Invalid DateTo format";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var DoctorScheduleQueryable = _unitOfWork.DoctorSchedules.FindAllQueryable(a => a.StartDate.Date <= FromDate.Date && (a.EndDate >= ToDate || a.EndDate == null), new[] { "Doctor", "WeekDay" }).AsQueryable();
                if (filters.SpecialtyID != null)
                {
                    DoctorScheduleQueryable = DoctorScheduleQueryable.Where(a => a.DoctorSpecialityId == filters.SpecialtyID);
                }
                if (filters.DoctorID != null) 
                {
                    DoctorScheduleQueryable = DoctorScheduleQueryable.Where(a => a.DoctorId == filters.DoctorID);
                }

                var DoctorScheduleList = DoctorScheduleQueryable.ToList();
                var ScheduleGroupByDoctorName = DoctorScheduleList.GroupBy(a => a.Doctor).ToList();

                var schedulePerWeekDataList = new List<GetDoctorSchedulePerWeekDTO>();
                var ScheduleDaysList = new ScheduleDaysListDTO();

                var usersIDsList = DoctorScheduleList.Select(a => a.Doctor.UserId).ToList();
                var userTeamsData = _unitOfWork.UserTeams.FindAll(a => usersIDsList.Contains(a.UserId), new[] { "Team" }).ToList();

                foreach (var Doc in ScheduleGroupByDoctorName)
                {
                    var currentUserTeam = userTeamsData.Where(a => a.UserId == Doc.Key.UserId).FirstOrDefault();

                    var schedulePerWeekData = new GetDoctorSchedulePerWeekDTO();
                    schedulePerWeekData.DoctorName = Doc.Key.FirstName + " " + Doc.Key.LastName;
                    schedulePerWeekData.DoctorSpecialityID = currentUserTeam?.TeamId;
                    schedulePerWeekData.DoctorSpecialityName = currentUserTeam?.Team?.Name;
                    var ScheduleDaysListData = new List<ScheduleDaysListDTO>();

                    

                    var startDay = FromDate.Date;
                    while(startDay <= ToDate.Date)
                    {
                        var dayName = startDay.ToString("dddd");
                        var dayData = Doc.Where(a => a.WeekDay.Day.ToLower() == dayName.ToLower() && a.StartDate.Date <= startDay && (a.EndDate == null || a.EndDate >= startDay)).ToList();

                        var ScheduleDay = new ScheduleDaysListDTO();
                        ScheduleDay.DayDate = startDay.ToString("yyyy-MM-dd");

                        var listOfSchInThisDay = new List<ScheduleListDTO>();
                        foreach (var sch in dayData)
                        {
                            var ScheduleListOfTheDay = new ScheduleListDTO();
                            ScheduleListOfTheDay.Id = sch.Id;
                            ScheduleListOfTheDay.Capacity = sch.Capacity;
                            ScheduleListOfTheDay.DateFrom = sch.StartDate.ToString("yyyy-MM-dd");
                            ScheduleListOfTheDay.DateTo = sch.EndDate?.ToString("yyyy-MM-dd");
                            ScheduleListOfTheDay.IntervalFrom = sch.IntervalFrom.ToString();
                            ScheduleListOfTheDay.IntervalTo = sch.IntervalTo.ToString();
                            ScheduleListOfTheDay.WeekDayID = sch.WeekDayId;
                            ScheduleListOfTheDay.DayName = sch.WeekDay.Day;

                            listOfSchInThisDay.Add(ScheduleListOfTheDay);
                        }
                        ScheduleDaysListData.Add(ScheduleDay);
                        ScheduleDay.ScheduleList = listOfSchInThisDay;

                        startDay = startDay.AddDays(1);
                    }

                    schedulePerWeekData.schedulaDaysList = ScheduleDaysListData;
                    schedulePerWeekDataList.Add(schedulePerWeekData);
                }
                response.Data = schedulePerWeekDataList;
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }
    }
}
