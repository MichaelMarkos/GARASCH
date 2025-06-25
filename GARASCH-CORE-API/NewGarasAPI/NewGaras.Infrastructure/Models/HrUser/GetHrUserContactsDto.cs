using NewGaras.Infrastructure.DTO.HrUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.HrUser
{
    public class GetHrUserContactsDto
    {
        public string Email { get; set; }
        public List<HrUserMobileDto> MobileNumbers { get; set; } = new List<HrUserMobileDto>();
        public List<HrUserLandLineDto> LandLines { get; set; } = new List<HrUserLandLineDto>();
        public List<HrUserSocialMediaDto> SocialMedias { get; set; } = new List<HrUserSocialMediaDto>();
    }
}
