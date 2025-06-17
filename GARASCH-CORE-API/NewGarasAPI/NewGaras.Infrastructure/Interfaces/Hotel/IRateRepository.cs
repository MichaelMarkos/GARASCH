


using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;

namespace NewGaras.Infrastructure.Interfaces.Hotel
{
    public interface IRateRepository : IBaseRepository<Rate , int>
    {
        BaseResponseWithData<List<Rate>> AddSpecialOffer(NewRateDto newRate);
        BaseResponseWithData<List<Rate>> DailyUpdate();
        public BaseResponseWithData<Rate> rateroomandoffers(int roomId);
        Task<BaseResponseWithData<List<SpecialOfferFlag>>> GetoffersforRoom(DurationDto2 durationDto , int roomId);
        Task<BaseResponse> AddRate(List<AddRateDto> newRates);
        Task<BaseResponseWithData<List<RatelistRoomDto2>>> RateListRoom(RatelistRoomDto dto);

    }
}
