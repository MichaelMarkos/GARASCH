

using NewGaras.Infrastructure.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewGaras.Infrastructure.DTO.Hotel.DTOs
{
    public class RoomDto2
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int RoomTypeId { get; set; }

        public int BuildingId { get; set; }

        public int RoomViewId { get; set; }

        public string Description { get; set; }

        public int? capacity { get; set; }


       
    }
}
