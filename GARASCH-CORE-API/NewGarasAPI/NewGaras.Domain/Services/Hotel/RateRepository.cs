using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Mappers.Hotel;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;



namespace NewGaras.Domain.Services.Hotel
{
    public class RateRepository : BaseRepository<Rate, int>, IRateRepository
    {
        protected GarasTestContext _context;
        protected IMapper _mapper;
        public RateRepository(GarasTestContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public BaseResponseWithData<List<Rate>> DailyUpdate()
        {
            BaseResponseWithData<List<Rate>> Response = new BaseResponseWithData<List<Rate>>();
            Response.Data = new List<Rate>();
            Response.Errors = new List<Error>();
            Response.Result = false;
            try
            {
                var activeRates = _context.Rates.Where(r => r.IsActive).ToList();
                foreach (var activeRate in activeRates)
                {
                    if (!activeRate.IsDefault && DateTime.Today > activeRate.EndingDate)
                    {
                        activeRate.IsActive = false;
                        _context.Rates.FirstOrDefault(r => r.RoomId == activeRate.RoomId ).IsActive = true;
                    }
                    Response.Result = true;
                    Response.Data.Add(activeRate);
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

        //public BaseResponseWithData<List<Rate>> AddSpecialOffer(NewRateDto newRates)
        //{
        //    BaseResponseWithData<List<Rate>> Response = new BaseResponseWithData<List<Rate>>();
        //    Response.Data = new List<Rate>();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;
        //    try
        //    {
        //        if (newRates.EndingDate < newRates.StartingDate)
        //        {
        //            Response.Errors.Add(new Error { code = "E-2", message = "EndingDate must be after StartingDate" });
        //            return Response;
        //        }
        //        if (newRates.RoomsIds.Count != newRates.RoomsIds.Count)
        //        {
        //            Response.Errors.Add(new Error { code = "E-2", message = "Rooms list does not match Rates list" });
        //            return Response;
        //        }
        //        for (int i = 0; i < newRates.RoomsIds.Count; i++)
        //        {
        //            var tempRate = new Rate //_mapper.Map<Rate>(newRates);
        //            {
        //                IsDefault = false,
        //                StartingDate = newRates.StartingDate,
        //                EndingDate = newRates.EndingDate,
        //                SpecialOfferFlag = newRates.SpecialOfferFlag,
        //                RoomTypeId = newRates.RoomTypeId,
        //                RoomViewId = newRates.RoomViewId,
        //                BuildingId = newRates.BuildingId
        //            };
        //            var oldOffers = _context.Rates.Where(r => r.RoomId == newRates.RoomsIds[i] && r.IsActive).ToList();
        //            foreach (var r in oldOffers)
        //            {
        //                r.IsActive = false;
        //            }
        //            tempRate.RoomId = newRates.RoomsIds[i];
        //            tempRate.RoomRate = newRates.RoomsRates[i];
        //            Response.Data.Add(tempRate);
        //            _context.Add(tempRate);
        //            Response.Result = true;
        //        }


        //        return Response;
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return Response;
        //    }
        //}

        public BaseResponseWithData<List<Rate>> AddSpecialOffer(NewRateDto newRates)
        {
            BaseResponseWithData<List<Rate>> Response = new BaseResponseWithData<List<Rate>>();
            Response.Data = new List<Rate>();
            Response.Errors = new List<Error>();
            Response.Result = false;
            try
            {
                if (newRates.EndingDate < newRates.StartingDate)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "EndingDate must be after StartingDate";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (newRates.RoomsIds.Count != newRates.RoomsRates.Count)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Rooms list does not match Rates list";
                    Response.Errors.Add(error);
                    return Response;
                }
                for (int i = 0; i < newRates.RoomsIds.Count; i++)
                {

                    var tempRate = new Rate //_mapper.Map<Rate>(newRates);
                    {
                        IsDefault = false,
                        StartingDate = newRates.StartingDate,
                        EndingDate = newRates.EndingDate,
                        SpecialOfferFlag = newRates.SpecialOfferFlag,
                        RoomTypeId = newRates.RoomTypeId,
                        RoomViewId = newRates.RoomViewId,
                        BuildingId = newRates.BuildingId
                    };
                    var oldOffers = _context.Rates.Where(r => r.RoomId == newRates.RoomsIds[i] && r.IsActive).ToList();
                    if (_context.Rates.Where(x => x.RoomId == newRates.RoomsIds[i] && x.StartingDate == newRates.StartingDate && x.EndingDate == newRates.EndingDate).Any())
                    {
                        foreach (var r in oldOffers)
                        {
                            r.IsActive = false;
                        }

                    }
                    tempRate.RoomId = newRates.RoomsIds[i];
                    tempRate.RoomRate = newRates.RoomsRates[i];
                    Response.Data.Add(tempRate);
                    _context.Add(tempRate);
                    Response.Result = true;



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

        public async Task<BaseResponse> AddRate(List<AddRateDto> newRates)
        {
            //BaseResponseWithData<List<AddRateDto>> Response = new BaseResponseWithData<List<AddRateDto>>();
            //Response.Data = new List<AddRateDto>();
            var Response = new BaseResponse();
            Response.Errors = new List<Error>();
            Response.Result = true;
            try
            {
                var rateDBList = _context.Rates.Where(x => x.IsDefault && x.SpecialOfferFlag == 0).ToList();
                foreach (var item in newRates)
                {
                    if (item.EndingDate < item.StartingDate)
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "EndingDate must be after StartingDate";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var rateByRoomId = rateDBList.Where(x => x.RoomId == item.RoomsId && x.StartingDate < item.StartingDate && x.EndingDate > item.EndingDate).FirstOrDefault();
                    _context.Rates.Add(new Rate
                    {
                        RoomId = item.RoomsId,
                        StartingDate = item.StartingDate,
                        EndingDate = item.EndingDate,
                        SpecialOfferFlag = 1,
                        RoomTypeId = item.RoomTypeId,
                        RoomViewId = item.RoomViewId,
                        BuildingId = item.BuildingId,
                        IsDefault = false,
                        IsActive = true,
                        RoomRate = item.RoomRate
                    });
                    _context.SaveChanges();
                    //var ratemodel = new Rate() 
                    //{
                    //    Id = rateByRoomId.Id,
                    //    RoomId =rateByRoomId.RoomId,
                    //    StartingDate =rateByRoomId.StartingDate,
                    //    EndingDate=item.StartingDate,
                    //    SpecialOfferFlag=0,
                    //    RoomTypeId=rateByRoomId.RoomTypeId,
                    //    RoomViewId=rateByRoomId.RoomViewId,
                    //    BuildingId=rateByRoomId.BuildingId,
                    //    IsDefault=true,
                    //    IsActive=true,
                    //    RoomRate =rateByRoomId.RoomRate
                    //};
                    //_context.Rates.Update(ratemodel);


                    var ratemodel2 = new Rate()
                    {
                        //Id = rateByRoomId.Id,
                        RoomId = rateByRoomId.RoomId,
                        StartingDate = item.EndingDate,
                        EndingDate = rateByRoomId.EndingDate,
                        SpecialOfferFlag = 0,
                        RoomTypeId = rateByRoomId.RoomTypeId,
                        RoomViewId = rateByRoomId.RoomViewId,
                        BuildingId = rateByRoomId.BuildingId,
                        IsDefault = true,
                        IsActive = true,
                        RoomRate = rateByRoomId.RoomRate
                    };
                    _context.Rates.Add(ratemodel2);
                    _context.SaveChanges();
                    rateByRoomId.EndingDate = item.StartingDate;
                    _context.Rates.Update(rateByRoomId);
                    _context.SaveChanges();
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



        public BaseResponseWithData<Rate> rateroomandoffers(int roomId)
        {
            BaseResponseWithData<Rate> Response = new BaseResponseWithData<Rate>();
            Response.Data = new Rate();
            Response.Errors = new List<Error>();
            Response.Result = false;
            try
            {
                var activeRates = _context.Rates.Where(r => r.IsActive && r.IsDefault && r.RoomId == roomId).FirstOrDefault();
                Response.Data = activeRates;
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

        public async Task<BaseResponseWithData<List<RatelistRoomDto2>>> RateListRoom(RatelistRoomDto dto)
        {
            BaseResponseWithData<List<RatelistRoomDto2>> Response = new BaseResponseWithData<List<RatelistRoomDto2>>();
            Response.Data = new List<RatelistRoomDto2>();
            Response.Errors = new List<Error>();
            Response.Result = false;
            var tempdate2 = new List<RatelistRoomDto2>();
            try
            {
                var rateDBListWithDuration = _context.Rates.Where(x => x.StartingDate < dto.StartingDate && x.EndingDate > dto.EndingDate).ToList();
                var AllrateDbListActive = _context.Rates.Where(x => x.IsActive).ToList();
                var roomDbList = _context.Rooms.ToList();
                foreach (var item in dto.RoomsIds)
                {
                    var tempdate = new RatelistRoomDto2();
                    if (dto.EndingDate < dto.StartingDate)
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "EndingDate must be after StartingDate";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    tempdate.RoomId = item;
                    tempdate.RoomName = roomDbList.Where(x => x.Id == item).Select(y => y.Name).FirstOrDefault();
                    tempdate.Roomcapacity = roomDbList.Where(x => x.Id == item).Select(y => y.Capacity).FirstOrDefault();
                    var dateRoomRateList = new List<DateRoomRate>();

                    for (DateTime datetime = dto.StartingDate; datetime <= dto.EndingDate; datetime = datetime.AddDays(1))
                    {
                        var dateRoomRate = new DateRoomRate();

                        dateRoomRate.date = datetime;
                        dateRoomRate.RoomRate = AllrateDbListActive.Where(x => x.RoomId == item && x.StartingDate <= datetime && x.EndingDate >= datetime).Select(y => y.RoomRate).FirstOrDefault();
                        dateRoomRateList.Add(dateRoomRate);
                        tempdate.dateroomrate = dateRoomRateList;
                        //tempdate.dateroomrate.Add(dateRoomRate)
                    }
                    tempdate2.Add(tempdate);

                }
                Response.Data = tempdate2;
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

        public async Task<BaseResponseWithData<List<SpecialOfferFlag>>> GetoffersforRoom(DurationDto2 durationDto, int roomId)
        {
            BaseResponseWithData<List<SpecialOfferFlag>> Response = new BaseResponseWithData<List<SpecialOfferFlag>>();
            Response.Data = new List<SpecialOfferFlag>();
            Response.Errors = new List<Error>();
            Response.Result = false;
            try
            {
                var tempdataRates = await _context.Rates.
                        Where(x => x.RoomId == roomId && x.EndingDate > durationDto.StartDate && x.StartingDate < durationDto.EndDate).ToListAsync();

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
                var rating = tempdataRates.Select(c => c.ToSpecialOfferFlag()).ToList();
                //var tester = _context.Rates.Select(c => new SpecialOfferFlag { StartingDate = c.StartingDate }).ToList();
                //  var ratingggg = tempdataRates.Select(c => new SpecialOfferFlag { StartingDate = c.StartingDate }).ToList();

                Response.Data = rating;
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
