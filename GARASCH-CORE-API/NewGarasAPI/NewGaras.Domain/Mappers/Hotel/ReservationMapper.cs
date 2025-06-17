

using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Hotel.DTOs;

namespace NewGaras.Domain.Mappers.Hotel
{
    public static class ReservationMapper
    {
        public static ReservationGridDto ToReservationGridDto(this Reservation commentModel)
        {
            return new ReservationGridDto
            {
                Id=commentModel.Id ,
                //reservationDate = commentModel.reservationDate,
                reservationDate=commentModel.ReservationDate ,
                ToDate=commentModel.ToDate ,
                FromDate=commentModel.FromDate ,
                ClientId=commentModel.ClientId ,
                Provider=commentModel.Provider ,
                Confirmation=commentModel.Confirmation ,
                ClientName=commentModel.Client.Name

            };
        }
    }
}
