using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IApplicationVersionService
    {

        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithData<GetAppsVersionModel> GetApplicationVersion([FromHeader] string CompanyName);

        public BaseResponse UpdateApplicationVersion(GetAppsVersionModel newVersion);
    }
}
