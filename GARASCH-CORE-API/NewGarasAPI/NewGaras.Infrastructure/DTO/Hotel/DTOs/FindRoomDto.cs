

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class FindRoomDto
    {
        [FromHeader]
        public string? Name { get; set; }
        [FromHeader]
        public string? RoomType { get; set; }
        [FromHeader]

        public string? Building { get; set; }
        [FromHeader]

        public string? RoomView { get; set; }
        [FromHeader]

        public List<int>?  Facilties { get; set; }
        [FromHeader]

        public bool FilterByReserved { get; set; }
        [FromHeader]

        public bool Reserved { get; set; }
        [FromHeader]

        public DateTime FromDate { get; set; } = DateTime.Now;
        [FromHeader]

        public DateTime? ToDate { get; set; }

    }
}
