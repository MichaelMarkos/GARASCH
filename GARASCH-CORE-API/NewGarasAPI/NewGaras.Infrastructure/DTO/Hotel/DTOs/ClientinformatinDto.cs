

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class ClientinformatinDto
    {
        public int? Id { get; set; }
        public string Type { get; set; } = null!;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public int? Number { get; set; }
        public string? Image { get; set; }
        public IFormFile? File { get; set; }
        public long ClientId { get; set; }
    }
}
