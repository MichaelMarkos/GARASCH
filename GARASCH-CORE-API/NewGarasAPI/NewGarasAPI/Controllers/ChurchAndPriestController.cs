﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Church;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.DTO.ChurchAndPriest;
using NewGaras.Infrastructure.DTO.ChurchAndPriest.Filters;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.DTO.General;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ChurchAndPriestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        static string key;
        private GarasTestContext _Context;
        private readonly IMapper _mapper;
        private readonly ITenantService _tenantService;
        private readonly IChurchAndPriestService _churchAndPriestService;
        public ChurchAndPriestController(IUnitOfWork unitOfWork, IWebHostEnvironment host, IMapper mapper, ITenantService tenantService, IChurchAndPriestService churchAndPriestService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _churchAndPriestService = churchAndPriestService;
        }

        [HttpPost("AddNewPriest")]
        public BaseResponseWithId<long> AddNewPriest(AddNewPriestDTO dto)
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
                    response = _churchAndPriestService.AddNewPriest(dto, validation.userID);
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

        [HttpPost("EditPriest")]
        public BaseResponseWithId<long> EditPriest(EditPriestDTO dto)
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
                    response = _churchAndPriestService.EditPriest(dto, validation.userID);
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

        [HttpGet("GetPriestList")]
        public BaseResponseWithDataAndHeader<List<GetPriestsListDTO>> GetPriestList(GetPriestsListFilters filters)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetPriestsListDTO>>()
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
                    response = _churchAndPriestService.GetPriestList(filters);
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

        [HttpPost("AddNewChurch")]
        public BaseResponseWithId<long> AddNewChurch(AddChurchDTO dto)
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
                    response = _churchAndPriestService.AddNewChurch(dto);
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

        [HttpPost("EditChurch")]
        public BaseResponseWithId<long> EditChurch(EditChurchDTO dto)
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
                    response = _churchAndPriestService.EditChurch(dto);
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

        [HttpGet("GetChurchesList")]
        public BaseResponseWithDataAndHeader<List<GetChurchesListDTO>> GetChurchesList(GetChurchsListFilters filters)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetChurchesListDTO>>()
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
                    response = _churchAndPriestService.GetChurchesList(filters);
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

        [HttpPost("AddNewEparchy")]
        public BaseResponseWithId<int> AddNewEparchy(AddEparchyDTO dto)
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
                    response = _churchAndPriestService.AddNewEparchy(dto);
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

        [HttpGet("GetEparchyDDL")]
        public SelectDDLResponse GetEparchyDDL()
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
                    response = _churchAndPriestService.GetEparchyDDL();
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

        [HttpPost("EditEparchy")]
        public BaseResponseWithId<int> EditEparchy(EditEparchyDTO dto)
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
                    response = _churchAndPriestService.EditEparchy(dto);
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


        [HttpGet("GetHrUserPriestHistory")]
        public BaseResponseWithData<List<GetHrUserPriestHistoryDTO>> GetHrUserPriestHistory(GetHrUserPriestHistoryFilters filters)
        {
            var response = new BaseResponseWithData<List<GetHrUserPriestHistoryDTO>>()
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
                    response = _churchAndPriestService.GetHrUserPriestHistory(filters);
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

        [HttpGet("GetEparchyWithChurch")]
        public BaseResponseWithDataAndHeader<List<GetEparchyWithChurchDTO>> GetEparchyWithChurch(GetEparchyWithChurchFilters filters)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetEparchyWithChurchDTO>>()
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
                    response = _churchAndPriestService.GetEparchyWithChurch(filters);
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

        [HttpPost("DeleteEparchy")]
        public BaseResponseWithId<int> DeleteEparchy(GeneralDeleteDTO<int> dto)
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
                    response = _churchAndPriestService.DeleteEparchy(dto);
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

        [HttpPost("DeleteChurch")]
        public BaseResponseWithId<long> DeleteChurch(GeneralDeleteDTO<long> dto)
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
                    response = _churchAndPriestService.DeleteChurch(dto);
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

        [HttpPost("DeletePriest")]
        public BaseResponseWithId<long> DeletePriest(GeneralDeleteDTO<long> dto)
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
                    response = _churchAndPriestService.DeletePriest(dto);
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
