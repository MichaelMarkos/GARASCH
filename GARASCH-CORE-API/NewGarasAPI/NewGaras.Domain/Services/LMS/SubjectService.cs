

using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;

namespace NewGaras.Domain.Services.LMS
{
    public class SubjectService
    {
        private readonly IWebHostEnvironment _host;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IMailService _mailService;
        private readonly IUnitOfWork _unitOfWork;
        private GarasTestContext _Context;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HearderVaidatorOutput validation;
        public SubjectService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, GarasTestContext context,
                                         Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, INotificationService notificationService, IMailService mailService, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _Context = context;
            _environment = environment;
            _notificationService = notificationService;
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string BaseURL
        {
            get
            {
                var uri = _httpContextAccessor?.HttpContext?.Request;
                string Host = uri?.Scheme + "://" + uri?.Host.Value.ToString();
                return Host;
            }
        }


        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png", ".jpeg", ".svg" };
        private new List<string> _allowedResourcesExtenstions = new List<string> { ".pdf", ".docs" };
        private long _maxAllowedPosterSize = 15728640;


    }
}
