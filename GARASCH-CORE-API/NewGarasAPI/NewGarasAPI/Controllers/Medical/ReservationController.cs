using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany;

namespace NewGarasAPI.Controllers.Medical
{
    [Route("Medical/Reservation")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IPatientService _patientService;
        public ReservationController(IWebHostEnvironment host, ITenantService tenantService, IPatientService patientService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _patientService = patientService;
        }
    }
}
