using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class HrUserContactsDto
    {
        public long HrUserId { get; set; }

        public string Email { get; set; }

        public List<HrUserSocialMediaDto> SocialMediaList { get; set; } = new List<HrUserSocialMediaDto>();
        public List<HrUserMobileDto> HrUserMobiles { get; set; } = new List<HrUserMobileDto>();
        public List<HrUserLandLineDto> HrUserLandlines { get; set; } = new List<HrUserLandLineDto>();
    }
}
