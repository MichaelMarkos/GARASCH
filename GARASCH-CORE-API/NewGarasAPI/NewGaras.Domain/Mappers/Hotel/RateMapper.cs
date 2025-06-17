

using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Hotel.DTOs;

namespace NewGaras.Domain.Mappers.Hotel
{
    public static class RateMapper
    {
        public static SpecialOfferFlag ToSpecialOfferFlag(this Rate commentModel)
        {
            return new SpecialOfferFlag
            {
                StartingDate=commentModel.StartingDate ,
                EndingDate=commentModel.EndingDate ,
                SpecialOfferFlags=commentModel.SpecialOfferFlag


            };
        }
    }
}
