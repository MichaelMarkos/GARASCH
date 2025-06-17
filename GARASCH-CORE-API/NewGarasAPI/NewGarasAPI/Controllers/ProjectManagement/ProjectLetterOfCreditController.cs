using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Domain.Services;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.ProjectLetterOfCredit;
using NewGaras.Infrastructure.DTO.ProjectPayment;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.DTO.TaskMangerProject;
using Azure;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using IronSoftware.Drawing;
using NewGaras.Infrastructure.Entities;
using Font = System.Drawing.Font;
using NewGaras.Infrastructure.Models.ProjectLetterOfCredit;
using NewGaras.Infrastructure.Models;
using System.Linq.Expressions;
using Grpc.Core;

namespace NewGarasAPI.Controllers.ProjectManagement
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectLetterOfCreditController : ControllerBase
    {
        //private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        static string key;
        private GarasTestContext _Context;
        private readonly IMapper _mapper;
        private readonly ITenantService _tenantService;
        private readonly ITaskMangerProjectService _taskMangerProjectService;

        public ProjectLetterOfCreditController(IWebHostEnvironment host, IMapper mapper,
            IHrUserService hrUserService, IMailService mailService,ITenantService tenantService, ITaskMangerProjectService taskMangerProjectService)
        {
            //_unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _taskMangerProjectService = taskMangerProjectService;
        }

        //---------------------------------------project letter of credite -------------------------------------
        [HttpPost("AddProjectLetterOfCredit")]              
        public BaseResponseWithId<long> AddProjectLetterOfCredit([FromBody] AddProjectLetterOfCreditDto Dto)
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

            #region validation
            if (Dto.ProjectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.LetterOfCreditTypeID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Letter Of Credit Type ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.CurrencyID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Currency ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            
            if (Dto.ReturnedAfter == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "ReturnedAfter Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(Dto.bankName))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Bank Name Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(Dto.StartDate))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Start Date Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Amount == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Amount Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(Dto.EndDate))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "End Date Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            #region Date Validation
            DateTime startDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.StartDate, out startDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid Start Date:";
                response.Errors.Add(err);
                return response;
            }

            DateTime EndDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.EndDate, out EndDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid End Date:";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            //#region DB check
            //var project = _Context.Projects.Where(a => a.Id == Dto.ProjectID).Include(a=>a.SalesOffer).FirstOrDefault();
            //if (project == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Project with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //var currency = _Context.Currencies.Find(Dto.CurrencyID);
            //if (currency == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No currency with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //var letterOfCredit = _Context.LetterOfCreditTypes.Find(Dto.LetterOfCreditTypeID);
            //if (letterOfCredit == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Letter Of Credit Type with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            
            //#endregion

            try
            {
                if (response.Result)
                {
                    //var newProjectLOC = new ProjectLetterOfCredit();
                    //newProjectLOC.ProjectId = Dto.ProjectID;
                    //newProjectLOC.LetterOfCreditTypeId = Dto.LetterOfCreditTypeID;
                    //newProjectLOC.ReturnedAfter = Dto.ReturnedAfter;
                    //newProjectLOC.BankName = Dto.bankName;
                    //newProjectLOC.StartDate = startDate;
                    //newProjectLOC.Amout = Dto.Amount;
                    //newProjectLOC.CurrencyId = Dto.CurrencyID;
                    //newProjectLOC.EndDate = EndDate;
                    //newProjectLOC.Status = "Not Returned";
                    //newProjectLOC.CreatedBy = validation.userID;
                    //newProjectLOC.CreationDate = DateTime.Now;
                    //newProjectLOC.ModifiedBy = validation.userID;
                    //newProjectLOC.ModificationDate = DateTime.Now;
                    //newProjectLOC.Active = true;

                    //_Context.ProjectLetterOfCredits.Add(newProjectLOC);
                    //_Context.SaveChanges();


                    //response.ID = newProjectLOC.Id;
                    //return response;
                    var ProjectLetterOfCredit = _taskMangerProjectService.AddProjectLetterOfCredit(Dto, validation.userID);
                    if (!ProjectLetterOfCredit.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(ProjectLetterOfCredit.Errors);
                        return response;
                    }
                    response.ID = ProjectLetterOfCredit.ID;
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

        [HttpGet("GetProjectLetterOfCredit")]               
        public BaseResponseWithDataAndHeader<List<GetProjectLetterOfCreditDto>> GetProjectLetterOfCredit([FromHeader] ProjectLetterOfCreditGetModel request)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetProjectLetterOfCreditDto>>()
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
                    var projectLetterOfCreditList = _taskMangerProjectService.GetProjectLetterOfCredit(request);
                    if(projectLetterOfCreditList.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(projectLetterOfCreditList.Errors);
                        return response;
                    }
                    response = projectLetterOfCreditList;
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

        [HttpPost("EditProjectLetterOfCredit")]            
        public BaseResponseWithId<long> EditProjectLetterOfCredit([FromBody] EditProjectLetterOfCreditDto Dto)
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

            #region validation
            if (Dto.ID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Project Letter Of Credit ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.ProjectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.LetterOfCreditTypeID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Letter Of Credit Type ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.CurrencyID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Currency ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            
            if (Dto.ReturnedAfter == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "ReturnedAfter Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(Dto.bankName))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Bank Name Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(Dto.StartDate))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Start Date Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Amount == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Amount Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(Dto.EndDate))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "End Date Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion
            #region Date Validation
            DateTime startDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.StartDate, out startDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid Start Date:";
                response.Errors.Add(err);
                return response;
            }

            DateTime EndDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.EndDate, out EndDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid End Date:";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var projectLetterOfCreditList = _taskMangerProjectService.EditProjectLetterOfCredit(Dto, validation.userID);
                    if (projectLetterOfCreditList.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(projectLetterOfCreditList.Errors);
                        return response;
                    }
                    response = projectLetterOfCreditList;
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

        [HttpPost("EditProjectLetterOfCreditStatus")]       
        public BaseResponseWithId<long> EditProjectLetterOfCreditStatus([FromForm] long ProjectLetterOfCredit, [FromForm] bool status)
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

            #region validation
            if (ProjectLetterOfCredit == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Project Letter Of Credit ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            #region Check in DB
            var ProjectLOCDb = _Context.ProjectLetterOfCredits.Find(ProjectLetterOfCredit);
            if (ProjectLOCDb == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project Letter Of Credits with This ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    //if (status == true) ProjectLOCDb.Status = "Returned";
                    //else
                    //{
                    //    ProjectLOCDb.Status = "Returned";
                    //}
                    //_Context.Update(ProjectLOCDb);
                    //_Context.SaveChanges();

                    //response.ID = ProjectLOCDb.Id;
                    var ProjectLOC = _taskMangerProjectService.EditProjectLetterOfCreditStatus(ProjectLetterOfCredit, status);
                    if (!ProjectLOC.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(ProjectLOC.Errors);
                        return response;
                    }
                    response.ID = ProjectLOC.ID;
                }
                
                return response;

            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpPost("AddProjectLetterOfCreditComment")]       
        public BaseResponseWithId<List<long>> AddProjectLetterOfCreditComment([FromForm] AddLetterOfCreditCommentList comments)
        {
            var response = new BaseResponseWithId<List<long>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if(comments.CommentList == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = $"The Comment List is Empty , please add at least one comment";
                response.Errors.Add(error);
                return response;
            }

            int count = 1;
            foreach (var projectComment in comments.CommentList)
            {
                #region validation
                //var ProjectLetterOfCredit = _unitOfWork.ProjectLetterOfCredits.GetById(ProjectLetterOfCreditID);
                if (projectComment.ProjectLetterOfCreditID == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = $"ProjectLetterOfCreditID is requried at index number {count}";
                    response.Errors.Add(error);
                    return response;
                }
                if (string.IsNullOrEmpty(projectComment.comment))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = $"comment is requried at index number {count}";
                    response.Errors.Add(error);
                    return response;
                }
                #endregion

                
                count++;
            }

            try
            {
                if (response.Result)
                {
                    
                    var ProjectLOC = _taskMangerProjectService.AddProjectLetterOfCreditCommentList(comments, validation.userID);
                    if (!ProjectLOC.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(ProjectLOC.Errors);
                        return response;
                    }
                    response = ProjectLOC;

                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpPost("EditProjectLetterOfCreditComment")]      
        public BaseResponseWithId<long> EditProjectLetterOfCreditComment([FromForm] long CommentID, [FromForm] long prjectLetterOfCreditID, [FromForm] string comment)
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

            #region validation

            if (prjectLetterOfCreditID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "ProjectLetterOfCreditID is requried";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(comment))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "comment is requried";
                response.Errors.Add(error);
                return response;
            }

            #endregion

            #region DB check
            //var project = _Context.ProjectLetterOfCredits.FirstOrDefault(a => a.Id == prjectLetterOfCreditID);
            //if (project == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Project Letter Of Credits with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //var commentDb = _Context.ProjectLetterOfCreditComments.Find(CommentID);
            //if (commentDb == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Project Letter Of Credits comment with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            #endregion

            try
            {
                if (response.Result)
                {

                    var projectLetterOfCreditList = _taskMangerProjectService.EditProjectLetterOfCreditComment(CommentID, prjectLetterOfCreditID, comment, validation.userID);
                    if (projectLetterOfCreditList.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(projectLetterOfCreditList.Errors);
                        return response;
                    }
                    response = projectLetterOfCreditList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }

        }

        [HttpGet("GetProjectLetterOfCreditCommentList")]
        public BaseResponseWithData<List<GetProjectLetterOfCreditCommentDto>> GetProjectLetterOfCreditCommentList([FromHeader] long ProjectLetterOfCreditID)
        {
            var response = new BaseResponseWithData<List<GetProjectLetterOfCreditCommentDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (ProjectLetterOfCreditID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "ProjectLetterOfCreditID is requried";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    //var commentList = _Context.ProjectLetterOfCreditComments.Where(a => a.ProjectLetterOfCreditId == ProjectLetterOfCreditID).Include(a => a.CreatedByNavigation).ToList();

                    //var data = _mapper.Map<List<GetProjectLetterOfCreditCommentDto>>(commentList);
                    //response.Data = data;

                    var commentList = _taskMangerProjectService.GetProjectLetterOfCreditComment(ProjectLetterOfCreditID);
                    if (commentList.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(commentList.Errors);
                        return response;
                    }
                    response = commentList;
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

        [HttpGet("GetLetterOfCreditTypeDDL")]
        public BaseResponseWithData<List<CostTypeDDL>> GetLetterOfCreditTypeDDL()
        {
            var response = new BaseResponseWithData<List<CostTypeDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    var costType = _taskMangerProjectService.GetLetterOfCreditTypeDDL();
                    if (costType.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(costType.Errors);
                        return response;
                    }
                    response = costType;

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



















































































        [HttpGet("GetProjectLetterOfCreditReport")]
        public BaseResponseWithData<string> GetProjectLetterOfCreditReport([FromHeader] ProjectLetterOfCreditGetModel request)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            try
            {
                if (response.Result)
                {
                    response = _taskMangerProjectService.GetProjectLetterOfCreditReport(request, validation.CompanyName);
                }
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }
        /*private static float GetTextWidth(string text, Font font)
        {
        using (var graphics = Graphics.FromImage(new Bitmap(1, 1)))
        {
            var size = graphics.MeasureString(text, font);
            return size.Width;
        }
        }
        private static ExcelRange GetColumnRange(ExcelWorksheet worksheet, int columnNumber)
    {
        // Get the dimension of the worksheet
        var dimension = worksheet.Dimension;

        // Check if the worksheet is empty
        if (dimension == null)
        {
            return null;
        }

        // Get the range of cells in the specified column
        var startCell = worksheet.Cells[dimension.Start.Row, columnNumber];
        var endCell = worksheet.Cells[dimension.End.Row, columnNumber];

        return worksheet.Cells[startCell.Start.Row, startCell.Start.Column, endCell.End.Row, endCell.End.Column];
    }*/
    }
}
