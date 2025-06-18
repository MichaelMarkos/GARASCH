using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.DTO.Family.Filters;

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
        private readonly IFamilyService _familyService;
        public FamilyController(IUnitOfWork unitOfWork, IWebHostEnvironment host, IMapper mapper, ITenantService tenantService, IFamilyService familyService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _familyService = familyService;
        }

        [HttpPost("AddFamilyStatus")]
        public BaseResponseWithId<int> AddFamilyStatus(AddFamilyStatusDTO dto)
        {
            var response = new BaseResponseWithId<int>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response =  _familyService.AddFamilyStatus(dto);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetFamilyStatusDDL")]
        public SelectDDLResponse GetFamilyStatusDDL()
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _familyService.GetFamilyStatusDDL();
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpPost("AddFamily")]
        public BaseResponseWithId<long> AddFamily(AddFamilyDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _familyService.AddFamily(dto);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetFamiliesList")]
        public BaseResponseWithData<List<GetFamiliesListDTO>> GetFamiliesList([FromHeader]GetFamiliesListFilters filters)
        {
            var response = new BaseResponseWithData<List<GetFamiliesListDTO>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _familyService.GetFamiliesList(filters);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetFamilyByID")]
        public BaseResponseWithData<GetFamiliesListDTO> GetFamilyByID([FromHeader]long familyID)
        {
            var response = new BaseResponseWithData<GetFamiliesListDTO>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _familyService.GetFamilyByID(familyID);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

    }
}
