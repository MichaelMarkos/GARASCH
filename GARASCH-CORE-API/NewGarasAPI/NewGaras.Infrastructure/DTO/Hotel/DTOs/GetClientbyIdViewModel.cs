

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class GetClientbyIdViewModel
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; } 
        public string? Mobile { get; set; }
        public string? Nationality { get; set; }
      public string  ImagePath { get; set; }

    }

}
