using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Hotel.DTOs.Auth
{
    public class AuthModel
    {
        public long UserId { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<RoleModel> Roles { get; set; }
        public List<int> RoleIds { get; set; }

        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string? ImagePath { get; set; }
        public string? Phone { get; set; }
    }
    public class RoleModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
