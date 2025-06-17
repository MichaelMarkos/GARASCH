using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.DTO.Email;
using NewGaras.Infrastructure.Models.EmailTool.UsInResponses;

namespace NewGaras.Infrastructure.Models.EmailTool
{
    public class EmailBodyRsponse
    {
        public List<GetEmailByIdDto> EmailsList { get; set; }
    }
}
