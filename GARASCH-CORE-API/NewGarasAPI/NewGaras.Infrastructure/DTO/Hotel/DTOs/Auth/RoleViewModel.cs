using System;
using System.Collections.Generic;


namespace NewGaras.Infrastructure.DTO.Hotel.DTOs.Auth
{
    public class RoleViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public long? CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool Active { get; set; }
    }
}
