


namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class RatelistRoomDto
    {
        [FromHeader]
        public DateTime StartingDate { get; set; }
        [FromHeader]

        public DateTime EndingDate { get; set; }
        [FromHeader]

        public List<int> RoomsIds { get; set; }

    }
}
