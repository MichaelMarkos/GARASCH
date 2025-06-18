using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Helper.TenantService;

namespace NewGarasAPI.Controllers.HR
{
    [Route("[controller]")]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        static string key;
        private GarasTestContext _Context;
        private readonly IMapper _mapper;
        private readonly ITenantService _tenantService;
        public FamilyController(IUnitOfWork unitOfWork, IWebHostEnvironment host, IMapper mapper, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            
        }


    }
}
