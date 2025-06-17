using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Mappers.Hotel;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Hotel.DTOs;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Hotel.DTOs;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Repositories;
using NewGarasAPI.Models.User;
using System.Numerics;


namespace NewGaras.Domain.Services.Hotel
{
    public class ReservationRepository : BaseRepository<Reservation, int>, IReservationRepository
    {
        protected GarasTestContext _context;
        protected IMapper _mapper;
        public ReservationRepository(GarasTestContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public BaseResponseWithData<List<Reservation>> GetRoomReservationsfortesting(DurationDto? durationDto, int roomId = 0, int reservationId = 0)
        {
            BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
            Response.Data = new List<Reservation>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (durationDto.StartDate > durationDto.EndDate)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "End date must be before start date";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (roomId > 0)
                {
                    var temp = new List<Reservation>();

                    var room = _context.RoomsReservations.Where(X => X.RoomId == roomId).Any();
                    var reservationNum = _context.RoomsReservations.Where(x => x.RoomId == roomId && x.ReservationId != reservationId).Select(x => x.Reservation).ToList();
                    for (int i = 0; i < reservationNum.Count; i++)
                    {
                        var tempdata = _context.Reservations.Where(x => x.Id == reservationNum[i].Id && x.ToDate > durationDto.StartDate && x.FromDate < durationDto.EndDate).ToList();
                        if (tempdata.Count != 0)
                        {
                            //int y = 0;
                            //temp[y] = tempdata;
                            //y++;
                            Response.Result = true;
                            Response.Data = tempdata;

                            return Response;


                        }

                    }
                }

                return Response;




            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<Reservation>> GetRoomReservations(DurationDto? durationDto, int roomId = 0)
        {
            BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
            Response.Data = new List<Reservation>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (durationDto.StartDate > durationDto.EndDate)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "End date must be before start date";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (roomId > 0)
                {
                    var room = _context.RoomsReservations.Where(X => X.RoomId == roomId).Any();
                    Response.Data = _context.Reservations.Where(r => (
                                     r.ToDate > durationDto.StartDate
                                    && r.FromDate < durationDto.EndDate) && room).ToList();
                }
                else
                {
                    Response.Data = _context.Reservations.Where(r => r.ToDate >= durationDto.StartDate
                                    && r.FromDate <= durationDto.EndDate).ToList();
                }

                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<Reservation>> AllReservedRooms(DurationDto2? durationDto, int roomId = 0)
        {
            BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
            Response.Data = new List<Reservation>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (durationDto.StartDate > durationDto.EndDate)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "End date must be before start date";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (roomId > 0)
                {
                    //var test = new List<Reservation>();
                    var rooms = _context.RoomsReservations.Where(X => X.RoomId == roomId).Select(x => x.Reservation).ToList();
                    Response.Data = rooms.Where(r => r.ToDate >= durationDto.StartDate && r.FromDate <= durationDto.EndDate).ToList();
                    //foreach (var room in rooms)
                    //{
                    //    test = _context.Reservations.Where(r => (
                    //                      r.ToDate > durationDto.StartDate
                    //                     && r.FromDate < durationDto.EndDate) && r.Id == room.Id).ToList();
                    //    Response.Data.Add(test);

                    //}
                }
                else
                {
                    Response.Data = _context.Reservations.Where(r => r.ToDate >= durationDto.StartDate
                                    && r.FromDate <= durationDto.EndDate).ToList();
                }

                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<ReservationRoomsDto>> GetAllRoomReservations(DurationDto? durationDto, List<int> roomIds)
        {
            BaseResponseWithData<List<ReservationRoomsDto>> Response = new BaseResponseWithData<List<ReservationRoomsDto>>();
            Response.Data = new List<ReservationRoomsDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (durationDto.StartDate > durationDto.EndDate)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "End date must be before start date";
                    Response.Errors.Add(error);
                    return Response;
                }
                for (int i = 0; roomIds.Count > i; i++)
                {
                    if (roomIds[i] > 0)
                    {
                        var room = _context.RoomsReservations.Where(X => X.RoomId == roomIds[i]).Any();
                        var tempdata = _context.Reservations.Where(r => (
                                          r.ToDate >= durationDto.StartDate
                                         && r.FromDate <= durationDto.EndDate) && room).ToList();

                        Response.Data = GetAllWithRooms(_mapper.Map<List<ReservationRoomsDto>>(tempdata));
                    }

                    else
                    {
                        var tempdata = _context.Reservations.Where(r => r.ToDate >= durationDto.StartDate
                                         && r.FromDate <= durationDto.EndDate).ToList();
                        Response.Data = GetAllWithRooms(_mapper.Map<List<ReservationRoomsDto>>(tempdata));

                    }
                }
                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<RoomReservationDto>> GetRoomReservationsGrid(DurationDto2 durationDto, int? roomTypeId, int? roomviewId, int? roombuildingId)
        {
            BaseResponseWithData<List<RoomReservationDto>> Response = new BaseResponseWithData<List<RoomReservationDto>>();
            Response.Data = new List<RoomReservationDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                List<RoomReservationDto> roomReservationDto = new List<RoomReservationDto>();
                var allRooms = new List<Room>();
                if (roomTypeId == 0 && roomviewId == 0 && roombuildingId == 0)
                {
                    allRooms = _context.Rooms.ToList();
                }
                else if (roomTypeId > 0 && roomviewId == 0 && roombuildingId == 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomType.Id == roomTypeId).ToList();
                }
                else if (roomTypeId > 0 && roomviewId > 0 && roombuildingId == 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomType.Id == roomTypeId && x.RoomView.Id == roomviewId).ToList();
                }
                else if (roomTypeId > 0 && roomviewId > 0 && roombuildingId > 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomType.Id == roomTypeId && x.RoomView.Id == roomviewId && x.Building.Id == roombuildingId).ToList();
                }
                else if (roomTypeId == 0 && roomviewId > 0 && roombuildingId == 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomView.Id == roomviewId).ToList();
                }
                else if (roomTypeId == 0 && roomviewId > 0 && roombuildingId > 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomView.Id == roomviewId && x.Building.Id == roombuildingId).ToList();
                }
                else if (roomTypeId == 0 && roomviewId == 0 && roombuildingId > 0)
                {
                    allRooms = _context.Rooms.Where(x => x.Building.Id == roombuildingId).ToList();
                }
                else
                {
                    allRooms = _context.Rooms.Where(x => x.RoomType.Id == roomTypeId && x.RoomView.Id == roomviewId && x.Building.Id == roombuildingId).ToList();
                }
                foreach (var room in allRooms)
                {
                    RoomReservationDto tempRoomReservation = new RoomReservationDto();
                    tempRoomReservation.Room = _mapper.Map<RoomDto2>(room);
                    //var test = _context.RoomsReservations.Where(x => x.RoomId == room.Id).Select(y => y.Reservation.Id).ToList();
                    var tempdata = _context.RoomsReservations.Include(x => x.Reservation.Client).Where(x => x.RoomId == room.Id).Select(x => x.Reservation).ToList();
                    var tempdata1 = tempdata.Where(r => r.ToDate > durationDto.StartDate && r.FromDate < durationDto.EndDate).ToList();
                    tempRoomReservation.reservations = tempdata1.Select(c => c.ToReservationGridDto()).ToList();
                    // tempRoomReservation.ClientName = _context.Clients.FirstOrDefault(x => x.Id == tempRoomReservation.reservations[0].ClientId).Name;

                    var tempdataRates = _context.Rates.
                        Where(x => x.RoomId == room.Id && x.EndingDate > durationDto.StartDate && x.StartingDate < durationDto.EndDate).ToList();
                    foreach (var item in tempdataRates)
                    {
                        if (tempdataRates != null)
                        {
                            if (item.IsDefault == false && durationDto.StartDate > item.StartingDate)
                            {
                                item.StartingDate = durationDto.StartDate;
                            }
                            else if (item.IsDefault == false && item.EndingDate > durationDto.EndDate)
                            {
                                item.EndingDate = durationDto.EndDate;
                            }
                        }
                    }
                    tempRoomReservation.rate = tempdataRates.Select(c => c.ToRateDto()).ToList();

                    //tempRoomReservation.reservations = _context.Reservations
                    //    .Where(r =>(
                    //                r.ToDate > durationDto.StartDate
                    //               && r.FromDate < durationDto.EndDate) 
                    //               && _context.RoomsReservations.Where(x=>x.RoomId==room.Id) ).ToList();
                    roomReservationDto.Add(tempRoomReservation);
                }
                Response.Data = roomReservationDto;
                Response.Result = true;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }



        //public BaseResponseWithData<List<RoomReservationDto>> GetRoomReservationsGrid(DurationDto? durationDto)
        //{
        //    BaseResponseWithData<List<RoomReservationDto>> Response = new BaseResponseWithData<List<RoomReservationDto>>();
        //    Response.Data = new List<RoomReservationDto>();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;
        //    List<RoomReservationDto> roomReservationDto = new List<RoomReservationDto>();

        //    try
        //    {
        //        var allRooms = _context.Rooms.ToList();
        //        foreach (var room in allRooms)
        //        {
        //            RoomReservationDto tempRoomReservation = new RoomReservationDto();
        //            tempRoomReservation.Room = room;
        //            tempRoomReservation.reservations = _context.Reservations.Where(r => r.RoomId == room.Id
        //                            && r.ToDate > durationDto.StartDate
        //                            && r.FromDate < durationDto.EndDate).ToList();
        //            roomReservationDto.Add(tempRoomReservation);
        //        }
        //        Response.Data = roomReservationDto;
        //        Response.Result = true;
        //        return Response;

        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return Response;
        //    }
        //}
        public List<ReservationRoomsDto> GetAllWithRooms(List<ReservationRoomsDto> roomDtos)
        {

            for (int i = 0; i < roomDtos.Count; i++)
            {

                roomDtos[i] = GetWithRoom(roomDtos[i]);
            }
            return roomDtos;
        }

        public ReservationRoomsDto GetWithRoom(ReservationRoomsDto roomDto)
        {
            var Rooms = _context.RoomsReservations.Where(x => x.ReservationId == roomDto.Id)
                    .Select(x => x.Room).ToList();
            var allRate = _context.Rates.Where(x => x.Id > 0).ToList();

            roomDto.Rooms = Rooms.Select(a => new RoomModel
            {
                Id = a.Id,
                Name = a.Name,
                rate = allRate.FirstOrDefault(r => r.IsActive && r.RoomId == a.Id).RoomRate
            }).ToList();


            //List<int> Rates = new List<int>();

            //for (int y = 0; y < Rooms.Count; y++)
            //{
            //    var tempdata = _context.Rates.FirstOrDefault(r => r.IsActive && r.RoomId == (roomDto.Rooms[y].Id)).RoomRate;
            //    Rates.Add(tempdata);
               

            //    roomDto.Rate = Rates;

            //}

            //roomDto.Rate = _context.Rates.FirstOrDefault(r => r.IsActive && r.RoomId == roomDto.Rooms.Select(x => x.Id).FirstOrDefault()).RoomRate;
            // roomDto.Rate[i] = 1000;
            //roomDto.Rate =(List<int>) _context.Rates.Where(r => r.IsActive &&r.RoomId==roomDto.Rooms.Select(x=>x.Id).ToList() ).RoomRate;
            return roomDto;
        }

        public ReservationRoomsByIDDto GetWithRoomandmealandchildern(ReservationRoomsByIDDto roomDto)
        {
            var Rooms = _context.RoomsReservations.Where(x => x.ReservationId == roomDto.Id)
                    .Select(x => x.Room.Id).ToList();

            var alldata = new List<AddlistreservationByIDDto>();
            if (Rooms.Count > 0)
            {
                //{
                var RoomResvChildrensDBList = _context.RoomsReservationChilderns.Where(x => x.ReservationId == roomDto.Id).ToList();
                var RoomsReservationsDBlist = _context.RoomsReservations.Where(x => x.ReservationId == roomDto.Id).Include(y => y.Room).ToList();
                var MealName = _context.RoomsReservationMeals.Where(x => x.ReservationId == roomDto.Id).Select(x => x.MealType.Name).FirstOrDefault();
                var ChildernsDBlist = _context.Childerns.Where(x => x.ReservationId == roomDto.Id).ToList();
                foreach (var RoomId in Rooms)
                {
                    var alltempdata = new AddlistreservationByIDDto();
                    alltempdata.NumAdults = RoomResvChildrensDBList.Where(x => x.RoomId == RoomId).Select(x => x.NumbersofAdulte).FirstOrDefault();
                    //alltempdata.RoomName = _context.RoomsReservations.Where(x => x.ReservationId == roomDto.Id && x.RoomId == RoomId)
                    //                    .Select(x => x.Room.Name).FirstOrDefault();
                    alltempdata.RoomName = RoomsReservationsDBlist.Where(x => x.RoomId == RoomId)
                                        .Select(x => x.Room.Name).FirstOrDefault();
                    alltempdata.RoomId = RoomsReservationsDBlist.Where(x => x.RoomId == RoomId)
                                        .Select(x => x.Room.Id).FirstOrDefault();
                    alltempdata.MealName = MealName;
                    alltempdata.NumChildern = ChildernsDBlist.Where(x => x.RoomId == RoomId).Count();
                    alltempdata.YearsofChildern = ChildernsDBlist.Where(x => x.RoomId == RoomId).Select(x => x.Years).ToList();

                    alldata.Add(alltempdata);
                }
                //for (int i = 0; Rooms.Count < i; i++)
                //    alltempdata.NumAdults = _context.RoomsReservationChilderns.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i]).Select(x => x.NumbersofAdulte).First();
                //    alltempdata.RoomName = _context.RoomsReservations.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i])
                //            .Select(x => x.Room.Name).FirstOrDefault();
                //    alltempdata.RoomId = _context.RoomsReservations.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i])
                //           .Select(x => x.Room.Id).FirstOrDefault();
                //    alltempdata.MealName = _context.RoomsReservationMeals.Where(x => x.ReservationId == roomDto.Id).Select(x => x.MealType.Name).FirstOrDefault();
                //    alltempdata.NumChildern = _context.Childerns.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i]).Count();
                //    alltempdata.YearsofChildern = _context.Childerns.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i]).Select(x => x.Years).ToList();

                //    alldata.Add(alltempdata);

                //}
                roomDto.Reservationdetails = alldata;

            }
            return roomDto;
        }


        //public ReservationRoomsByIDDto GetWithRoomandmealandchildern(ReservationRoomsByIDDto roomDto)
        //{
        //    var Rooms = _context.RoomsReservations.Where(x => x.ReservationId == roomDto.Id)
        //            .Select(x => x.Room.Id).ToList();
        //    if (Rooms.Count > 0)
        //    {
        //        for (int i = 0; Rooms.Count > i; i++)
        //        {
        //            roomDto.addlist[i].NumAdults = _context.RoomsReservationChilderns.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i]).Select(x => x.NumbersofAdulte).FirstOrDefault();

        //            string test = _context.RoomsReservations.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i])
        //                    .Select(x => x.Room.Name).FirstOrDefault();
        //            roomDto.addlist[i].RoomName = test;
        //            roomDto.addlist[i].MealName = _context.RoomsReservationMeals.Where(x => x.ReservationId == roomDto.Id).Select(x => x.MealType.Name).FirstOrDefault();
        //            roomDto.addlist[i].NumChildern = _context.Childerns.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i]).Count();
        //            int counter = _context.Childerns.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i]).Count();
        //            roomDto.addlist[i].YearsofChildern = _context.Childerns.Where(x => x.ReservationId == roomDto.Id && x.RoomId == Rooms[i]).Select(x => x.Years).ToList();

        //        }
        //    }
        //    return roomDto;
        //}


        //    //roomDto.Rate = _context.Rates.FirstOrDefault(r => r.IsActive && r.RoomId == roomDto.Rooms.Select(x => x.Id).FirstOrDefault()).RoomRate;
        //    // roomDto.Rate[i] = 1000;
        //    //roomDto.Rate =(List<int>) _context.Rates.Where(r => r.IsActive &&r.RoomId==roomDto.Rooms.Select(x=>x.Id).ToList() ).RoomRate;
        //    return roomDto;
        //}

        public BaseResponseWithData<Reservation> AddRooms(int id, List<AddListofReservationDto> addlist, bool updateFacility = false)
        {
            BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
            Response.Data = new Reservation();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                for (int i = 0; addlist.Count > i; i++)
                {
                    if (id == 0 || addlist[i].RoomId is 0 || addlist[i].RoomId is EmptyResult)
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Invalid room reservation";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                if (updateFacility == true)
                {
                    foreach (var roomReservation in _context.RoomsReservations.Where(x => x.ReservationId == id))
                    {
                        _context.RoomsReservations.Remove(roomReservation);
                    }
                    foreach (var roomReservation in _context.RoomsReservationMeals.Where(x => x.ReservationId == id))
                    {
                        _context.RoomsReservationMeals.Remove(roomReservation);
                    }
                    foreach (var roomReservation in _context.RoomsReservationChilderns.Where(x => x.ReservationId == id))
                    {
                        _context.RoomsReservationChilderns.Remove(roomReservation);
                    }
                    foreach (var roomReservation in _context.Childerns.Where(x => x.ReservationId == id))
                    {
                        _context.Childerns.Remove(roomReservation);
                    }

                }
                //var RoomsReservations = new List<RoomsReservation>();
                //var RoomsReservationMeals = new List<RoomsReservationMeal>();
                //var RoomsReservationChilderns = new List<RoomsReservationChildern>();
                //for (int i = 0; addlist.Count > i; i++)
                //{
                //    var RoomsReservation = new RoomsReservation();
                //    var RoomsReservationMeal = new RoomsReservationMeal();
                //    var RoomsReservationChildern = new RoomsReservationChildern();
                //    RoomsReservation = _context.RoomsReservations.Add(new RoomsReservation { RoomId = addlist[i].RoomId, ReservationId = id });
                //    RoomsReservationMeal =_context.RoomsReservationMeals.Add(new RoomsReservationMeal { MealTypeId = addlist[i].MealId, RoomId = addlist[i].RoomId, ReservationId = id });
                //    RoomsReservationChildern = _context.RoomsReservationChilderns.Add(new RoomsReservationChildern { NumbersofAdulte = addlist[i].NumbersofAud, RoomId = addlist[i].RoomId, ReservationId = id });
                //    if (addlist[i].Years != null)
                //    {
                //        for (int y = 0; addlist[i].Years.Count > y; y++)
                //        {
                //            _context.Childerns.Add(new Childern { RoomId = addlist[i].RoomId, ReservationId = id, Years = addlist[i].Years[y] });
                //        }
                //    }
                //}
                var roomsReservations = new List<RoomsReservation>();
                var roomsReservationMeals = new List<RoomsReservationMeal>();
                var roomsReservationChildren = new List<RoomsReservationChildern>();

                for (int i = 0; i < addlist.Count; i++)
                {
                    var roomId = addlist[i].RoomId;
                    var mealId = addlist[i].MealId;
                    var numberOfAdults = addlist[i].NumbersofAud;

                    var reservation = new RoomsReservation
                    {
                        RoomId = roomId,
                        ReservationId = id
                    };
                    _context.RoomsReservations.Add(reservation);
                    roomsReservations.Add(reservation);

                    if (addlist[i].MealId != null)
                    {
                        var meal = new RoomsReservationMeal
                        {
                            MealTypeId = (int)mealId,
                            RoomId = roomId,
                            ReservationId = id
                        };
                        _context.RoomsReservationMeals.Add(meal);
                        roomsReservationMeals.Add(meal);
                    }


                    var children = new RoomsReservationChildern
                    {
                        NumbersofAdulte = numberOfAdults,
                        RoomId = roomId,
                        ReservationId = id
                    };
                    _context.RoomsReservationChilderns.Add(children);
                    roomsReservationChildren.Add(children);

                    if (addlist[i].Years != null)
                    {
                        foreach (var year in addlist[i].Years)
                        {
                            var child = new Childern
                            {
                                RoomId = roomId,
                                ReservationId = id,
                                Years = year
                            };
                            _context.Childerns.Add(child);
                        }
                    }
                }

                _context.SaveChanges();

                if (updateFacility == true)
                {
                    Response.Data = Find(x => x.Id == id);

                }

                Response.Result = true;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        //public BaseResponseWithData<Reservation> AddRooms(int id, List<int> RoomIds, bool updateFacility = false)
        //{
        //    BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
        //    Response.Data = new Reservation();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;

        //    try
        //    {
        //        if (id == 0 || RoomIds is null || RoomIds.Contains(0))
        //        {
        //            Response.Errors.Add(new Error { code = "E-2", message = "Invalid room reservation" });
        //            return Response;
        //        }
        //        if (updateFacility == true)
        //        {
        //            foreach (var roomReservation in _context.RoomsReservations.Where(x => x.ReservationId == id))
        //            {
        //                _context.RoomsReservations.Remove(roomReservation);
        //            }

        //        }
        //        foreach (var room in RoomIds)
        //        {
        //            _context.RoomsReservations.Add(new RoomsReservation { RoomId = room, ReservationId = id });

        //        }
        //        Response.Data = Find(x => x.Id == id);
        //        Response.Result = true;
        //        return Response;

        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return Response;
        //    }
        //}

        public BaseResponseWithData<List<ReservationRoomsDto>> GetAllRoomReservationsForAdding(DurationDto? durationDto, List<int> roomIds)
        {
            BaseResponseWithData<List<ReservationRoomsDto>> Response = new BaseResponseWithData<List<ReservationRoomsDto>>();
            Response.Data = new List<ReservationRoomsDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (durationDto.StartDate > durationDto.EndDate)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "End date must be before start date";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (roomIds.Count == 0 || roomIds == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Enter numbers of rooms";
                    Response.Errors.Add(error);
                    return Response;
                }
                for (int i = 0; roomIds.Count > i; i++)
                {
                    if (roomIds[i] > 0)
                    {
                        //var room = _context.RoomsReservations.Where(X => X.RoomId == roomIds[i]).Any();
                        //var temp = _context.Reservations.Where(r => (
                        //                  r.ToDate >= durationDto.StartDate
                        //                 && r.FromDate <= durationDto.EndDate) && room).ToList();
                        var temp = _context.Reservations.Where(r => (
                                          r.ToDate >= durationDto.StartDate
                                         && r.FromDate <= durationDto.EndDate)).ToList();
                        // var tempdata = _context.RoomsReservations.Where(r=>r.ReservationId == )
                        Response.Data = GetAllWithRooms(_mapper.Map<List<ReservationRoomsDto>>(temp));
                    }


                }
                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public BaseResponseWithData<List<Reservation>> NotGetRoomReservations(DurationDto? durationDto, List<int> roomIds, int Id)
        {
            BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
            Response.Data = new List<Reservation>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (durationDto.StartDate > durationDto.EndDate)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "End date must be before start date";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (roomIds.Count > 0)
                {
                    foreach (var roomId in roomIds)
                    {
                        //var room = _context.RoomsReservations.Where(X => X.RoomId == roomId &&X.ReservationId==Id).Any();
                        var tempdata = _context.Reservations.Where(r => (
                                          r.ToDate > durationDto.StartDate
                                         && r.FromDate < durationDto.EndDate) &&
                                         _context.RoomsReservations.Where(X => X.RoomId == roomId && X.ReservationId != Id).Any()).ToList();

                        Response.Data = tempdata;
                    }

                }

                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public BaseResponseWithData<List<Room>> GetRoomsNotReservationsGrid(DurationDto2 durationDto, int? roomTypeId, int? roomviewId, int? roombuildingId)
        {
            BaseResponseWithData<List<Room>> Response = new BaseResponseWithData<List<Room>>();
            Response.Data = new List<Room>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                var allRooms = new List<Room>();
                var AllRooms = new List<Room>();
                if (roomTypeId == 0 && roomviewId == 0 && roombuildingId == 0)
                {
                    allRooms = _context.Rooms.ToList();
                }
                else if (roomTypeId > 0 && roomviewId == 0 && roombuildingId == 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomType.Id == roomTypeId).ToList();
                }
                else if (roomTypeId > 0 && roomviewId > 0 && roombuildingId == 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomType.Id == roomTypeId && x.RoomView.Id == roomviewId).ToList();
                }
                else if (roomTypeId > 0 && roomviewId > 0 && roombuildingId > 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomType.Id == roomTypeId && x.RoomView.Id == roomviewId && x.Building.Id == roombuildingId).ToList();
                }
                else if (roomTypeId == 0 && roomviewId > 0 && roombuildingId == 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomView.Id == roomviewId).ToList();
                }
                else if (roomTypeId == 0 && roomviewId > 0 && roombuildingId > 0)
                {
                    allRooms = _context.Rooms.Where(x => x.RoomView.Id == roomviewId && x.Building.Id == roombuildingId).ToList();
                }
                else if (roomTypeId == 0 && roomviewId == 0 && roombuildingId > 0)
                {
                    allRooms = _context.Rooms.Where(x => x.Building.Id == roombuildingId).ToList();
                }
                else
                {
                    allRooms = _context.Rooms.Where(x => x.RoomType.Id == roomTypeId && x.RoomView.Id == roomviewId && x.Building.Id == roombuildingId).ToList();
                }
                //foreach (var room in allRooms)
                //{
                //    //var test = _context.RoomsReservations.Where(x => x.RoomId == room.Id).Select(y => y.Reservation.Id).ToList();
                //    var tempdata = _context.RoomsReservations.Where(x => x.RoomId == room.Id).Select(x => x.Reservation).ToList();
                //    if (tempdata.Where(r => r.ToDate >= durationDto.StartDate && r.FromDate <= durationDto.EndDate).Any())
                //    {
                //        allRooms.Remove(room);
                //    }
                //    // tempRoomReservation.ClientName = _context.Clients.FirstOrDefault(x => x.Id == tempRoomReservation.reservations[0].ClientId).Name;
                //}
                for (int i = 0; allRooms.Count > i; i++)
                {
                    //var test = _context.RoomsReservations.Where(x => x.RoomId == room.Id).Select(y => y.Reservation.Id).ToList();
                    var tempdata = _context.RoomsReservations.Where(x => x.RoomId == allRooms[i].Id).Select(x => x.Reservation).ToList();
                    if (!(tempdata.Where(r => r.ToDate >= durationDto.StartDate && r.FromDate <= durationDto.EndDate).Any()))
                    {
                        AllRooms.Add(allRooms[i]);
                    }
                    // tempRoomReservation.ClientName = _context.Clients.FirstOrDefault(x => x.Id == tempRoomReservation.reservations[0].ClientId).Name;
                }

                Response.Data = AllRooms;
                Response.Result = true;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }



        public BaseResponseWithData<int> GetAllRoomsReservationWithDuration(DurationDto2 durationDto)
        {
            BaseResponseWithData<int> Response = new BaseResponseWithData<int>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (durationDto.StartDate > durationDto.EndDate)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "End date must be before start date";
                    Response.Errors.Add(error);
                    return Response;
                }


                var ReservationIds = _context.Reservations.Where(r => (
                                  r.ToDate >= durationDto.StartDate
                                 && r.FromDate <= durationDto.EndDate)).Select(r => r.Id).ToList();

                Response.Data = _context.RoomsReservations.Where(X => ReservationIds.Contains(X.ReservationId)).Count();


                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }



        public BaseResponseWithData<long> GetOfferId(ReservationDto dto, long createdBy)
        {
            BaseResponseWithData<long> Response = new BaseResponseWithData<long>();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (dto.FromDate > dto.ToDate)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "End date must be before start date";
                    Response.Errors.Add(error);
                    return Response;
                }


                var salesOffer = _context.SalesOffers.Add(new SalesOffer
                {
                    StartDate = DateOnly.FromDateTime(dto.FromDate),
                    EndDate = DateOnly.FromDateTime(dto.ToDate),
                    CreatedBy = createdBy,
                    OfferType = dto.OfferType,
                    SalesPersonId = createdBy,
                    CreationDate = DateTime.Now,
                    Active = true,
                    ClientId = dto.ClientId,
                    Completed = true,
                    OfferAmount = dto.TotalCost,
                    BranchId = _context.Branches.Where(x => x.Id > 0).Select(b => b.Id).FirstOrDefault(),
                });

                _context.SaveChanges();
                var test = salesOffer.Entity.Id;
                var salesOferProducts = new List<SalesOfferProduct>();
                foreach (var pro in dto.ListRooms)
                {
                    var salesOferProduct = new SalesOfferProduct
                    {
                        CreatedBy = createdBy,
                        CreationDate = DateTime.Now,
                        Active = true,
                        OfferId = salesOffer.Entity.Id,
                        Quantity = ((dto.FromDate.Day) - (dto.ToDate.Day)),
                        ItemPrice = pro.RoomRate,
                        FinalPrice = pro.RoomRate,
                        InventoryItemId = pro.InventoryItemId
                    };
                    salesOferProducts.Add(salesOferProduct);

                }

                _context.SalesOfferProducts.AddRange(salesOferProducts);
                _context.SaveChanges();


                Response.Result = true;
                Response.Data = salesOffer.Entity.Id;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
        public BaseResponse AddInvoice(ReservationInvoice dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Errors = new List<Error>();
            Response.Result = true;

            try
            {
                if (dto.Amount <= 0 || dto.CreateBy == 0 )
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "add Amount or CreateBy";
                    Response.Errors.Add(error);
                    return Response;
                }


                var ReservationInvoice = _context.ReservationInvoices.Add(new ReservationInvoice
                {
                    Amount = dto.Amount,
                    InvoiceDate = dto.InvoiceDate,
                    CreateBy = dto.CreateBy,
                    CreateDate = DateTime.Now,
                    Serial = dto.Serial,
                    IsClosed = true,
                    ClientId = dto.ClientId,
                    ReservationId = dto.ReservationId,
                    InvoiceTypeId = dto.InvoiceTypeId,
                    CurrencyId = dto.CurrencyId,
                });

                var reservation = _context.Reservations.FirstOrDefault(x => x.Id == dto.ReservationId);
                reservation.TotalPaid = reservation.TotalPaid + dto.Amount;
                _context.Reservations.Update(reservation);
            
                _context.SaveChanges();
              
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
        public BaseResponse AddAddons(AddonsDto dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Errors = new List<Error>();
            Response.Result = true;

            try
            {
                if (dto.Amount <= 0 )
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "add Amount";
                    Response.Errors.Add(error);
                    return Response;
                }

                var reservation = _context.Reservations.FirstOrDefault(x => x.Id == dto.ReservationId);
                reservation.TotalCost = reservation.TotalCost + dto.Amount;
                _context.Reservations.Update(reservation);
                var salesOffer = _context.SalesOffers.FirstOrDefault(x => x.Id == reservation.OfferId);
                salesOffer.FinalOfferPrice = reservation.TotalCost;
                salesOffer.OfferAmount = reservation.TotalCost;
                _context.SalesOffers.Update(salesOffer);

                var inventoryItem = _context.InventoryItems.FirstOrDefault(x=>x.Id == dto.InventoryItemId);

                var salesOferProduct = new SalesOfferProduct
                {
                    CreatedBy = dto.CreateBy,
                    CreationDate = DateTime.Now,
                    Active = true,
                    OfferId = salesOffer.Id,
                    Quantity = dto.Quantity,
                    ItemPrice = inventoryItem.CustomeUnitPrice,
                    FinalPrice =(inventoryItem.CustomeUnitPrice * (decimal)dto.Quantity),
                    InventoryItemId = dto.InventoryItemId,
                    ItemPricingComment = dto.ItemPricingComment
                };
                _context.SalesOfferProducts.Add(salesOferProduct);


                _context.SaveChanges();

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


    }
}
