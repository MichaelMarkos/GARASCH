using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class HrUserSocialMediaDto
    {
        public long ID { get; set; }

        public string Link { get; set; } = string.Empty;

        public string Type { get; set; }

    }
}
