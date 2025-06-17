using Azure;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.ProjectInvoiceCollected;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.ProjectInvoice;
using System.Collections.Generic;


namespace NewGarasAPI.Controllers.Project
{
    [Route("HR/[controller]")]
    [ApiController]
    public class ProjectInvoiceController
        : ControllerBase
    {
        private readonly IProjectInvoiceService _projectInvoiceService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;


        public ProjectInvoiceController(IProjectInvoiceService projectInvoiceService, GarasTestContext context)
        {
            _projectInvoiceService = projectInvoiceService;
            _helper = new Helper.Helper();
            _Context = context;
        }
        [HttpPost("AddNewProjectInvoice")]
        public BaseResponseWithId<long> AddNewProjectInvoice([FromForm] AddProjectInvoiceModel invoiceModel)
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
                    Response = _projectInvoiceService.AddNewProjectInvoice(invoiceModel, validation.userID, validation.CompanyName);
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

        [HttpPost("UpdateProjectInvoiceItems")]
        public BaseResponseWithId<long> UpdateProjectInvoiceItems([FromBody] UpdateProjectInvoiceItemsModel invoiceItems)
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
                    Response = _projectInvoiceService.UpdateProjectInvoiceItems(invoiceItems, validation.userID);
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

        [HttpGet("GetProjectInvoices")]
        public BaseResponseWithData<List<GetProjectInvoiceModel>> GetProjectInvoices([FromHeader] long? projectId)
        {
            BaseResponseWithData<List<GetProjectInvoiceModel>> Response = new BaseResponseWithData<List<GetProjectInvoiceModel>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _projectInvoiceService.GetProjectInvoices(projectId);
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
        [HttpGet("GetProjectInvoiceItems")]
        public BaseResponseWithDataAndHeader<GetProjectInvoiceItemsDataModel> GetProjectInvoiceItems([FromHeader] long InvoiceId, [FromHeader] int page = 1, [FromHeader] int size = 10)
        {
            BaseResponseWithDataAndHeader<GetProjectInvoiceItemsDataModel> Response = new BaseResponseWithDataAndHeader<GetProjectInvoiceItemsDataModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _projectInvoiceService.GetProjectInvoiceItems(InvoiceId, page, size);
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
        [HttpPost("AddNewInvoiceItem")]
        public BaseResponseWithId<long> AddNewInvoiceItem([FromBody] AddProjectInvoiceItemModel itemModel)
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
                    Response = _projectInvoiceService.AddInvoiceItem(itemModel, validation.userID);
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

        [HttpPost("AddProjectInvoiceCollected")]
        public BaseResponseWithId<long> AddProjectInvoiceCollected([FromForm] AddProjectInvoiceCollectedDto Dto)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();


            #region validation
            if (Dto.ProjectInvoiceId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "please add a valid ProjectInvoiceId";
                Response.Errors.Add(error);
                return Response;
            }
            if (Dto.Amount == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "please add a valid Amount";
                Response.Errors.Add(error);
                return Response;
            }
            if (string.IsNullOrEmpty(Dto.Status))
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "please add a valid Status";
                Response.Errors.Add(error);
                return Response;
            }

            #endregion

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var newProjInvoCollected = _projectInvoiceService.AddProjectInvoiceCollected(Dto, validation.userID, validation.CompanyName);
                    if (!newProjInvoCollected.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(newProjInvoCollected.Errors);
                    }
                    Response = newProjInvoCollected;
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

        [HttpGet("GetProjectInvoiceCollectedList")]
        public BaseResponseWithData<List<GetProjectInvoiceCollectedDto>> GetProjectInvoiceCollectedList([FromHeader] long projectInvoiceID)
        {
            var response = new BaseResponseWithData<List<GetProjectInvoiceCollectedDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (projectInvoiceID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please enter a valid projectInvoiceID";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                if(response.Result)
                {
                    var projectInvoiceCollectedList = _projectInvoiceService.GetProjectInvoiceCollectedList(projectInvoiceID);
                    if (!projectInvoiceCollectedList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(projectInvoiceCollectedList.Errors);
                    }
                    response = projectInvoiceCollectedList;
                }
                    return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

        [HttpPost("EditProjectInvoiceCollected")]
        public BaseResponseWithId<long> EditProjectInvoiceCollected([FromForm] EditProjectInvoiceCollectedDto Dto)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            #region validation
            if (Dto.Amount == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "please add a valid Amount";
                Response.Errors.Add(error);
                return Response;
            }
            if (string.IsNullOrEmpty(Dto.Status))
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "please add a valid Status";
                Response.Errors.Add(error);
                return Response;
            }

            #endregion

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var newProjInvoCollected = _projectInvoiceService.EditProjectInvoiceCollected(Dto, validation.userID, validation.CompanyName);
                    if (!newProjInvoCollected.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(newProjInvoCollected.Errors);
                    }
                    Response = newProjInvoCollected;
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

        [HttpGet("GetProjectFinancialData")]
        public BaseResponseWithData<GetProjectFinancialDataModel> GetProjectFinancialData([FromHeader] long ProjectId)
        {
            var response = new BaseResponseWithData<GetProjectFinancialDataModel>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            if (ProjectId == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please enter a valid ProjectId";
                response.Errors.Add(err);
                return response;
            }
            #endregion
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _projectInvoiceService.GetProjectFinancialData(ProjectId);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

        [HttpPost("DeleteProjectInvoiceItem")]
        public BaseResponseWithId<long> DeleteProjectInvoiceItem([FromHeader] long InvoiceItemId)
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
                    var newProjInvoCollected = _projectInvoiceService.DeleteProjectInvoiceItem(InvoiceItemId,validation.userID);
                    if (!newProjInvoCollected.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(newProjInvoCollected.Errors);
                    }
                    Response = newProjInvoCollected;
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

        [HttpPost("DeleteProjectInvoiceCollected")]
        public BaseResponseWithId<long> DeleteProjectInvoiceCollected([FromHeader]long ProjectInvoiceCollectedId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            if(ProjectInvoiceCollectedId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The project Invoice Collected Is Requried" ;
                Response.Errors.Add(error);
                return Response;
            }

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var delteProjInvoCollected = _projectInvoiceService.DeleteProjectInvoiceCollected(ProjectInvoiceCollectedId);
                    if (!delteProjInvoCollected.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(delteProjInvoCollected.Errors);
                    }
                    Response = delteProjInvoCollected;
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


        [HttpPost("DeleteProjectInvoice")]
        public BaseResponseWithId<long> DeleteProjectInvoice([FromHeader]long InvoiceId)
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
                    var delteProjInvoCollected = _projectInvoiceService.DeleteProjectInvoice(InvoiceId);
                    if (!delteProjInvoCollected.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(delteProjInvoCollected.Errors);
                    }
                    Response = delteProjInvoCollected;
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


        [HttpGet("GetProjectInvoicesReport")]
        public BaseResponseWithData<string> GetProjectInvoicesReport([FromHeader] long ProjectId, [FromHeader] decimal? Amount, [FromHeader] long? CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            if (ProjectId == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please enter a valid ProjectId";
                response.Errors.Add(err);
                return response;
            }
            #endregion
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _projectInvoiceService.GetProjectInvoicesReport(ProjectId,Amount,CreatorId,From,To,validation.CompanyName);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }
    }
}
