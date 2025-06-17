

using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Hotel.DTOs;

namespace NewGaras.Domain.Mappers.Hotel
{
    public static class RateGridMapper
    {
        public static RateDto ToRateDto(this Rate commentModel)
        {
            return new RateDto
            {
                Id = commentModel.Id,
                RoomId = commentModel.RoomId,
                StartingDate = commentModel.StartingDate,
                EndingDate = commentModel.EndingDate,
                RoomOfferRate = commentModel.RoomRate,
                IsActive = commentModel.IsActive,
                SpecialOfferFlag =commentModel.SpecialOfferFlag

    };
        }
    }
}
