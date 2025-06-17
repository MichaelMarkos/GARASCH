
namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class DurationDto2
    {
    [FromHeader]
        public DateTime StartDate { get; set; }
    [FromHeader]
        public DateTime EndDate { get; set; }
    //[FromHeader]
    //    public int? test { get; set; }
    //[FromHeader]
    //    public string testaa { get; set; }
    }
}
