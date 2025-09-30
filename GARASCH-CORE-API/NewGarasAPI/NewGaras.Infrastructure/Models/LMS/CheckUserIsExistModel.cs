using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.LMS
{
    public class CheckUserIsExistModel
    {
        public long? UserId { get; set; }
        public string? Email { get; set; }
       // public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NationalId { get; set; }
    }
}
