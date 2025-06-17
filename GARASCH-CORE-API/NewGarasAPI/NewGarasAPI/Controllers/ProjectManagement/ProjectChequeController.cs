using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.ProjectManagement;
using NewGarasAPI.Models.ProjectManagement;
using System.Collections.Generic;

namespace NewGarasAPI.Controllers.ProjectManagement
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectChequeController : ControllerBase
    {
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IProjectChequeService _projectChequeService;
        public ProjectChequeController(IMapper mapper, IWebHostEnvironment host,ITenantService tenantService, IProjectChequeService projectChequeService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            _mapper = mapper;
            _host = host;
            _projectChequeService = projectChequeService;
        }

        [HttpPost("AddNew")]
        public BaseResponseWithId<long> AddNewCheque([FromForm]ProjectChequeDto dto)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectChequeService.AddNewCheque(dto,validation.CompanyName,validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("AddNewList")]
        public async Task<BaseResponseWithId<long>> AddNewChequeList([FromForm] List<ProjectChequeDto> dto)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _projectChequeService.AddNewChequeList(dto, validation.CompanyName, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAll")]
        public BaseResponseWithDataAndHeader<GetAllProjectChequesResponse> GetAllProjectCheques(GetAllProjectChequesFilter filter)
        {
            BaseResponseWithDataAndHeader<GetAllProjectChequesResponse> Response = new BaseResponseWithDataAndHeader<GetAllProjectChequesResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {

                    Response = _projectChequeService.GetAllProjectCheques(filter,validation.CompanyName);
                    /*var Cheques = _Context.ProjectCheques.Include(a=>a.ChequeChashingStatus).Include(a=>a.Currency).Include(a=>a.WithdrawedByNavigation).AsQueryable();
                    if (!All) {
                        if (!string.IsNullOrEmpty(Bank))
                        {
                            Cheques = Cheques.Where(a => a.Bank.Trim().ToLower().Contains(Bank.Trim().ToLower())).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(Branch))
                        {
                            Cheques = Cheques.Where(a=>a.BankBranch.Trim().ToLower().Contains(Branch.Trim().ToLower())).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(clientName))
                        {
                            Cheques = Cheques.Where(a => a.ClientName.Trim().ToLower().Contains(clientName.Trim().ToLower())).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(ProjectName))
                        {
                            Cheques = Cheques.Where(a=>a.ProjectName.Trim().ToLower().Contains(ProjectName.Trim().ToLower())).AsQueryable();
                        }
                        if (cashingStatus != null)
                        {
                            Cheques = Cheques.Where(a => a.ChequeChashingStatusId == cashingStatus).AsQueryable();
                        }
                        if (month != null)
                        {
                            Cheques = Cheques.Where(a=>a.ChequeDate.Month==month).AsQueryable();
                        }
                        if(chequeStatusId!=null && chequeStatusId!=0)
                        {
                            if(chequeStatusId == 1) 
                            {
                                Cheques = Cheques.Where(a => a.ChequeChashingStatusId==null).AsQueryable();
                            }
                            else if(chequeStatusId == 2) 
                            {
                                Cheques = Cheques.Where(a => a.ChequeChashingStatusId==1).AsQueryable();
                            }
                            else if(chequeStatusId == 3) 
                            {
                                Cheques = Cheques.Where(a => a.ChequeChashingStatusId==2 || a.ChequeChashingStatusId==5).AsQueryable();
                            }
                            else if(chequeStatusId == 4) 
                            {
                                Cheques = Cheques.Where(a => a.ChequeChashingStatusId==6).AsQueryable();
                            }
                        }

                    }
                    var chequesList = Cheques.ToList();

                    var List = _mapper.Map<List<GetProjectChequeDto>>(chequesList);

                    var returnedList = PagedList<GetProjectChequeDto>.Create(List.AsQueryable(),pageNumber,pageSize);
                    Response.Data = returnedList;
                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = returnedList.CurrentPage,
                        TotalPages = returnedList.TotalPages,
                        ItemsPerPage = returnedList.PageSize,
                        TotalItems = returnedList.TotalCount
                    };
*/
                }
                return Response;
            }
            catch(Exception ex) 
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetChequeStatusDDL")]
        public BaseResponseWithData<List<GetChequeStatusModel>> GetChequeStatusDDL()
        {
            BaseResponseWithData<List<GetChequeStatusModel>> Response =  new BaseResponseWithData<List<GetChequeStatusModel>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectChequeService.GetChequeStatusDDL();

                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetById")]
        public BaseResponseWithData<GetProjectChequeDto> GetChequeById([FromHeader] long ChequeId)
        {
            BaseResponseWithData<GetProjectChequeDto> Response = new BaseResponseWithData<GetProjectChequeDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response=_projectChequeService.GetChequeById(ChequeId);
                }
                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("Update")]
        public BaseResponseWithId<long> UpdateCheque([FromForm] ProjectChequeDto dto)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _projectChequeService.UpdateCheque(dto, validation.CompanyName, validation.userID);
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("AddBankChequeTemplate")]
        public BaseResponseWithId<int> AddBankChequeTemplate([FromForm]BankChequeTemplatedto dto)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectChequeService.AddBankChequeTemplate(dto, validation.userID, validation.CompanyName);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("EditBankChequeTemplate")]
        public BaseResponseWithId<int> EditBankChequeTemplate([FromForm]BankChequeTemplatedto dto)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectChequeService.EditBankChequeTemplate(dto, validation.userID, validation.CompanyName);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }


        [HttpGet("GetChequeTemplateList")]
        public BaseResponseWithData<GetChequeTemplatesReponse> GetChequeTemplateList()
        {
            BaseResponseWithData<GetChequeTemplatesReponse> Response = new BaseResponseWithData<GetChequeTemplatesReponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectChequeService.GetChequeTemplateList();

                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
