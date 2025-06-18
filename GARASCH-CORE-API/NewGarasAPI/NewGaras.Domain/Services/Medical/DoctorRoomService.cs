using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Medical.DoctorRooms;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical;
using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NewGaras.Domain.Services.Medical
{
    public class DoctorRoomService : IDoctorRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        static readonly string key = "SalesGarasPass";
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
        public DoctorRoomService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
        }

        public BaseResponseWithId<long> AddDoctorRoom(DoctorRoomDto dto)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            #region validation
            if (dto.BranchID == null)
            {
                var branchDB = _unitOfWork.Branches.GetById(dto.BranchID ?? 0);
                if (branchDB == null)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No branch eith this ID";
                    Response.Errors.Add(err);
                    return Response;
                }
            }
            #endregion
            try
            {
                if (dto == null) 
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Invalid Data!";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Invalid Room Name!";
                    Response.Errors.Add(error);
                    return Response;
                }
                var newRoom = new DoctorRoom()
                {
                    Name = dto.Name,
                    Active = true,
                    CreatedBy = validation.userID,
                    CreationDate = DateTime.Now,
                    BranchId = dto.BranchID,
                };
                _unitOfWork.DoctorRooms.Add(newRoom);
                _unitOfWork.Complete();
                Response.ID = newRoom.Id;
                return Response;
            }
            catch (Exception ex) 
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> UpdateDoctorRoom(DoctorRoomDto dto)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region validation
                if (dto.BranchID == null)
                {
                    var branchDB = _unitOfWork.Branches.GetById(dto.BranchID ?? 0);
                    if (branchDB == null)
                    {
                        Response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "No branch eith this ID";
                        Response.Errors.Add(err);
                        return Response;
                    }
                }
                #endregion

                if (dto == null) 
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Invalid Data!";
                    Response.Errors.Add(error);
                    return Response;
                }
                var room = _unitOfWork.DoctorRooms.GetById(dto.Id);
                if(room == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Room Not Found!";
                    Response.Errors.Add(error);
                    return Response;
                }
                _mapper.Map<DoctorRoomDto,DoctorRoom>(dto, room);
                _unitOfWork.DoctorRooms.Update(room);
                _unitOfWork.Complete();
                Response.ID = dto.Id;
                return Response;
            }
            catch (Exception ex) 
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithDataAndHeader<GetDoctorRooms> GetDoctorRoomList(GetDoctorRoomListFilters filters)
        {
            BaseResponseWithDataAndHeader<GetDoctorRooms> Response = new BaseResponseWithDataAndHeader<GetDoctorRooms>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                GetDoctorRooms DoctorRooms = new GetDoctorRooms();
                var rooms = _unitOfWork.DoctorRooms.FindAllQueryable(a=>a.Active).AsQueryable();

                if (!string.IsNullOrWhiteSpace(filters.SearchKey))
                {
                    filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
                    rooms = rooms.Where(a=>a.Name.ToLower().Contains(filters.SearchKey)).AsQueryable();
                }
                if (filters.BranchID != null)
                {
                    rooms = rooms.Where(a => a.BranchId == filters.BranchID).AsQueryable();
                }
                var list = PagedList<DoctorRoom>.Create(rooms, filters.CurrentPage, filters.NumberOfItemsPerPage);
                var finalList = _mapper.Map<List<DoctorRoomDto>>(list);
                DoctorRooms.DoctorRoomsList = finalList;
                Response.Data = DoctorRooms;
                Response.PaginationHeader = new PaginationHeader()
                {
                    CurrentPage = list.CurrentPage,
                    ItemsPerPage = list.PageSize,
                    TotalItems = list.TotalCount,
                    TotalPages = list.TotalPages
                };
                return Response;
            }
            catch (Exception ex) 
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<DoctorRoomDto> GetDoctorRoomById([FromHeader] long RoomId)
        {
            BaseResponseWithData<DoctorRoomDto> Response = new BaseResponseWithData<DoctorRoomDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (RoomId <= 0) 
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Invalid Id";
                    Response.Errors.Add(error);
                    return Response;
                }
                var room = _unitOfWork.DoctorRooms.GetById(RoomId);
                if (room == null) 
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Room Not Found!";
                    Response.Errors.Add(error);
                    return Response;
                }
                var finalRoom = _mapper.Map<DoctorRoomDto>(room);
                Response.Data = finalRoom;
                return Response;
            }
            catch (Exception ex) 
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithDataAndHeader<RoomsWithSchedule> GetRoomsWithSchedule(GetRoomsWithScheduleFilters filters)
        {
            BaseResponseWithDataAndHeader<RoomsWithSchedule> Response = new BaseResponseWithDataAndHeader<RoomsWithSchedule>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region sort validation
                if (filters.SortByDoctornameAsce == true && filters.SortByDoctornameDesc == true)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E10";
                    err.ErrorMSG = "can not sort Ascending and Descending orders at the same time , choose only one";
                    Response.Errors.Add(err);
                    return Response;
                }
                #endregion

                RoomsWithSchedule roomsSchedules =  new RoomsWithSchedule();
                var rooms = _unitOfWork.DoctorRooms.FindAllQueryable(a =>a.Active, includes: new[] { "DoctorSchedules.Doctor", "DoctorSchedules.DoctorSpeciality", "DoctorSchedules.WeekDay", "DoctorSchedules.PercentageType", "DoctorSchedules.Status", "DoctorSchedules.Branch" }).AsQueryable();

                if (!string.IsNullOrWhiteSpace(filters.SearchKey))
                {
                    filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
                    rooms = rooms.Where(a=>a.Name.ToLower().Contains(filters.SearchKey.ToLower())).AsQueryable();
                }

                if (filters.SortByDoctornameAsce)
                {
                    rooms = rooms.OrderBy(a =>a.Name).AsQueryable();
                }

                if (filters.SortByDoctornameDesc)
                {
                    rooms = rooms.OrderByDescending(a => a.Name).AsQueryable();
                }

                if (filters.BranchID != null)
                {
                    rooms = rooms.Where(a => a.BranchId == filters.BranchID).AsQueryable();
                }

                /*if (filters.Day != null)
                {
                    rooms = rooms.Where(a=>a.DoctorSchedules.Any(x=>x.StartDate<=filters.Day && x.EndDate>=filters.Day)).AsQueryable();
                }*/
                var list = PagedList<DoctorRoom>.Create(rooms, filters.CurrentPage, filters.NumberOfItemsPerPage);

                var dayName = filters.Day.ToString("dddd");
                var finalList = list.Select(a=>new RoomWithSchedule()
                {
                    Id = a.Id,
                    Name = a.Name,
                    Schedules = a.DoctorSchedules.Where(a=> a.StartDate.Date <= filters.Day && (a.EndDate >= filters.Day || a.EndDate == null)
                    && a.WeekDay.Day.ToLower() == dayName.ToLower()).Select(a=>new RoomSchedule()
                    {
                        Capacity = a.Capacity,
                        Id = a.Id,
                        consultationPrice = a.ConsultationPrice,
                        ExaminationPrice = a.ExaminationPrice,
                        DoctorId = a.DoctorId,
                        DoctorName = a.Doctor.FirstName+" "+(a.Doctor.MiddleName!=null?a.Doctor.MiddleName+" ":null)+a.Doctor.LastName,
                        IntervalFrom = a.IntervalFrom,
                        IntervalTo = a.IntervalTo,
                        SpecialityId = a.DoctorSpecialityId,
                        SpecialityName = a.DoctorSpeciality.Name,
                        WeekdayId = a.WeekDayId,
                        WeekdayName = a.WeekDay.Day,
                        PercentageTypeId = a.PercentageTypeId,
                        PercentageTypeName = a.PercentageType.DoctorPercentageType,
                        DoctorStatusId = a.StatusId,
                        DoctorStatusName = a.Status.DoctorScheduleStatusType,
                        StartDate = a.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndDate = a.EndDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        //HoldQuantity = a.HoldQuantity??2,
                        BranchID = a.BranchId,
                        BranchName = a.Branch?.Name
                    }).ToList()

                }).ToList();
                roomsSchedules.Rooms = finalList;
                Response.Data = roomsSchedules;
                Response.PaginationHeader = new PaginationHeader()
                {
                    CurrentPage = list.CurrentPage,
                    ItemsPerPage = list.PageSize,
                    TotalItems = list.TotalCount,
                    TotalPages = list.TotalPages,
                };
                return Response;
            }
            catch (Exception ex) 
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
