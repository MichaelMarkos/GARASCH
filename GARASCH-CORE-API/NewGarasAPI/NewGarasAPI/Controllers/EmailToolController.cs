using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Email;
using NewGaras.Infrastructure.DTO.EmailTool;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.EmailTool;
using NewGaras.Infrastructure.Models.EmailTool.Filters;
using NewGaras.Infrastructure.Models.EmailTool.UsInResponses;
using NewGaras.Infrastructure.Models.TaskMangerProject;
using NewGarasAPI.Helper;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmailToolController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private readonly IEmailToolService _emailToolService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IGraphAuthService _authService;
        private readonly HttpClient _httpClient;

        public EmailToolController(IUnitOfWork unitOfWork, IWebHostEnvironment host, IEmailToolService emailToolService, ITenantService tenantService, IGraphAuthService authService)
        {
            _host = host;
            _unitOfWork = unitOfWork;
            _emailToolService = emailToolService;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);

            //---------------------new ----------------------------
            _authService = authService;
            _httpClient = new HttpClient();
        }

        //[HttpPost("AddListOfEmails")]
        //public BaseResponseWithId<List<long>> AddEmail([FromForm] AddListOfEmail dto)
        //{
        //    var response = new BaseResponseWithId<List<long>>()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };

        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.Errors = validation.errors;
        //    response.Result = validation.result;
        //    #endregion

        //    try
        //    {
        //        if (response.Result)
        //        {
        //            var ListOfIDs = new List<long>();
        //            var newEmails = _emailToolService.AddListOfEmails(dto, validation.userID, validation.CompanyName);
        //            if (!newEmails.Result)
        //            {
        //                response.Result = false;
        //                response.Errors.AddRange(newEmails.Errors);
        //                return response;
        //            }
        //            ListOfIDs.AddRange(newEmails.ID);
        //            response.ID = ListOfIDs;
        //            return response;
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error err = new Error();
        //        err.ErrorCode = "E-1";
        //        err.errorMSG = "Exception :" + ex.Message;
        //        response.Errors.Add(err);
        //        return response;
        //    }
        //}

        //[HttpGet("GetEmailById")]
        //public BaseResponseWithData<GetEmailByIdDto> GetEmailById([FromHeader] long? Id, [FromHeader] string emailID)
        //{
        //    var response = new BaseResponseWithData<GetEmailByIdDto>()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };


        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.Errors = validation.errors;
        //    response.Result = validation.result;
        //    #endregion

        //    try
        //    {
        //        if (response.Result)
        //        {
        //            var email = _emailToolService.GetEmailById(emailID, Id);
        //            if (!email.Result)
        //            {
        //                response.Result = false;
        //                response.Errors.AddRange(email.Errors);
        //                return response;
        //            }
        //            response = email;
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error err = new Error();
        //        err.ErrorCode = "E-1";
        //        err.errorMSG = "Exception :" + ex.Message;
        //        response.Errors.Add(err);
        //        return response;
        //    }
        //}

        //[HttpGet("GetAllEmails")]
        //public BaseResponseWithData<List<GetEmailByIdDto>> GetAllMails([FromHeader] GetMailsHeaders dto)
        //{
        //    var response = new BaseResponseWithData<List<GetEmailByIdDto>>()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };


        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.Errors = validation.errors;
        //    response.Result = validation.result;
        //    #endregion

        //    try
        //    {
        //        if (response.Result)
        //        {

        //            var emailsList = _emailToolService.GetAllMails(dto);
        //            if (!emailsList.Result)
        //            {
        //                response.Result = false;
        //                response.Errors.AddRange(emailsList.Errors);
        //                return response;
        //            }
        //            response = emailsList;
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error err = new Error();
        //        err.ErrorCode = "E-1";
        //        err.errorMSG = "Exception :" + ex.Message;
        //        response.Errors.Add(err);
        //        return response;
        //    }
        //}

        [HttpGet("TestNgrok")]
        public List<string> test()
        {
            var strList = new List<string>()
            {
                "first",
                "second"
            };
            return strList;
        }

        [HttpGet("GetEmailFromMicroSoft")]
        public async Task<BaseResponseWithData<string>> GetEmailFromMicroSoft([FromHeader] string clientId, [FromHeader] string tenantId)
        {
            var response = new BaseResponseWithData<string>()
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

                    var emailsList = await _emailToolService.TryGetEmails(clientId, tenantId);
                    if (!emailsList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(emailsList.Errors);
                        return response;
                    }
                    response = emailsList;
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

        [HttpGet("messages")] //Done
        public async Task<IActionResult> GetEmails([FromHeader] string userId)
        {
            try
            {
                // Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                // Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the request to Microsoft Graph
                var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{userId}/messages");
                if (response.IsSuccessStatusCode)
                {
                    var emails = await response.Content.ReadAsStringAsync();
                    return Ok(emails);
                }

                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //[HttpPost("send")]
        //public async Task<IActionResult> SendEmail([FromBody] EmailMessage emailMessage, [FromHeader] string userEmail)      //userEmail to be get from DB
        //{
        //    try
        //    {
        //        // Get access token
        //        string accessToken = await _authService.GetAccessTokenAsync();

        //        // Set up HTTP client with authorization
        //        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //        // Create the email content
        //        var emailContent = new
        //        {
        //            message = new
        //            {
        //                subject = emailMessage.Subject,
        //                body = new
        //                {
        //                    contentType = "Text",
        //                    content = emailMessage.Content
        //                },
        //                toRecipients = new[]
        //                {
        //            new
        //            {
        //                emailAddress = new
        //                {
        //                    address = emailMessage.RecipientEmail
        //                }
        //            }
        //        }
        //            }
        //        };

        //        var jsonContent = JsonConvert.SerializeObject(emailContent);
        //        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        //        // Replace {user-id} with the ID or email of the sender’s mailbox
        //        var response = await _httpClient.PostAsync($"https://graph.microsoft.com/v1.0/users/{userEmail}/sendMail", content);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            return Ok("Email sent successfully!");
        //        }

        //        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        //[HttpGet("GetEmailWithId")]
        //public async Task<IActionResult> GetEmailById([FromHeader]string EmailUserId, [FromHeader]string messageId)
        //{
        //    try
        //    {
        //        // Get access token
        //        string accessToken = await _authService.GetAccessTokenAsync();

        //        // Set up HTTP client with authorization
        //        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //        // Make the request to retrieve the specific email by message ID
        //        var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserId}/messages/{messageId}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var email = await response.Content.ReadAsStringAsync();
        //            return Ok(email);
        //        }

        //        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}


        [HttpGet("GetEmailAttachments")]
        public async Task<IActionResult> GetEmailAttachments([FromHeader] string userId, [FromHeader] string messageId)
        {
            try
            {
                // Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                // Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the request to get attachments for the specified email
                var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{userId}/messages/{messageId}/attachments");

                if (response.IsSuccessStatusCode)
                {
                    var attachments = await response.Content.ReadAsStringAsync();
                    return Ok(attachments);
                }

                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        //----------------------------------------APIs with services-------------------------------------------------
        [HttpGet("GetEmailWithId")]
        public async Task<BaseResponseWithData<GetEmailByIdDto>> GetEmailById([FromHeader] string EmailUserId, [FromHeader] string messageId)
        {
            var response = new BaseResponseWithData<GetEmailByIdDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            #region validation
            if (string.IsNullOrEmpty(messageId))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = $"please enter a valid message ID";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(EmailUserId))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = $"please enter a valid user ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var email = await _emailToolService.GetEmailById(EmailUserId, messageId);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetAllMessages")]
        public async Task<BaseResponseWithData<EmailBodyRsponse>> GetAllEmails([FromHeader] string EmailUserID)
        {
            var response = new BaseResponseWithData<EmailBodyRsponse>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            //#region validation

            //if (string.IsNullOrEmpty(EmailUserID))
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = $"please enter a valid user ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //#endregion

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            var toList  = new List<string>();
            toList.Add("patric.vetanoia@gmail.com");
            var CcList = new List<string>();
            toList.Add("marconagy7000@gmail.com");
            var emailMessage = new SendEmailMessage()
            {
                To = toList,
                Cc = CcList,
                Subject = "Subject test",
                Body = "Body test"
                //AttachmentsList = AttachmentsList
            };

            try
            {
                if (response.Result)
                {
                    var email = await _emailToolService.GetAllEmails(emailMessage, validation.userID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpPost("SendEmail")]
        public async Task<BaseResponseWithId<string>> SendEmail([FromForm] List<string> To, [FromForm] List<string> Cc, [FromForm] string Subject, [FromForm] string Body, [FromForm] List<AddAttachment> AttachmentsList)
        {
            var response = new BaseResponseWithId<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            #region validation

            //if (string.IsNullOrEmpty(emailMessage.UserID))
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = $"please enter a valid user ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            var emailMessage = new SendEmailMessage()
            {
                To = To,
                Cc = Cc,
                Subject = Subject,
                Body = Body,
                AttachmentsList = AttachmentsList
            };

            int count = 0;
            foreach (var message in emailMessage.To)
            {
                count++;
                if (string.IsNullOrEmpty(message))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = $"please enter a valid RecipientEmail at email number : {count}";
                    response.Errors.Add(error);
                    return response;
                }
            }

            int index = 0;
            foreach (var message in emailMessage.Cc)
            {
                index++;
                if (string.IsNullOrEmpty(message))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = $"please enter a valid CcEmail at email number : {index}";
                    response.Errors.Add(error);
                    return response;
                }
            }
            #endregion

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var email = await _emailToolService.SendEmail(emailMessage, validation.userID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetSpamEmails")]
        public async Task<BaseResponseWithData<EmailBodyRsponse>> GetSpamEmails()
        {
            #region old code
            //try
            //{
            //    // Get access token
            //    string accessToken = await _authService.GetAccessTokenAsync();

            //    // Set up HTTP client with authorization
            //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            //    // Make the request to retrieve emails from the Junk Email folder (spam folder)
            //    var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{userId}/mailFolders/JunkEmail/messages");

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var emails = await response.Content.ReadAsStringAsync();
            //        return Ok(emails);
            //    }

            //    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, $"Internal server error: {ex.Message}");
            //}
            #endregion

            var response = new BaseResponseWithData<EmailBodyRsponse>()
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
                    var email = await _emailToolService.GetSpamEmails(validation.userID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetJunkEmails")]
        public async Task<BaseResponseWithData<EmailBodyRsponse>> GetJunkEmails()
        {
            var response = new BaseResponseWithData<EmailBodyRsponse>()
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
                    var email = await _emailToolService.GetSpamEmails(validation.userID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetEmailAttachmentFile")]
        public async Task<BaseResponseWithData<EmailAttachmentDto>> DownloadAttachmentAsIFormFile([FromHeader] string userId, [FromHeader] string messageId)
        {
            var response = new BaseResponseWithData<EmailAttachmentDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            #region validation

            if (string.IsNullOrEmpty(userId))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = $"please enter a valid user ID";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(messageId))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = $"please enter a valid Message ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var email = await _emailToolService.DownloadAttachmentAsIFormFile(userId, messageId, validation.CompanyName);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpPost("SyncAllEmails")]
        public async Task<BaseResponseWithId<List<string>>> SyncAllEmails()
        {
            var response = new BaseResponseWithId<List<string>>()
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
                    var email = await _emailToolService.SyncAllEmails(validation.userID, validation.CompanyName);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetAllMessagesForUser")]
        public async Task<BaseResponseWithData<EmailBodyRsponse>> GetAllEmailsForUser()
        {
            var response = new BaseResponseWithData<EmailBodyRsponse>()
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
                    var email = await _emailToolService.GetAllEmailsForUser(validation.userID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetAllEmailsForUserDB")]
        public async Task<BaseResponseWithData<EmailBodyRsponseDTO>> GetAllEmailsForUserDB([FromHeader] GetAllEmailsForUserDBFilters filters)
        {
            var response = new BaseResponseWithData<EmailBodyRsponseDTO>()
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
                    var email = await _emailToolService.GetAllEmailsForUserDB(filters, validation.userID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetEmailByIdDB")]
        public async Task<BaseResponseWithData<GetEmailsListFromDBDto>> GetEmailByIdDB([FromHeader] long? EmailID, [FromHeader]string microSoftEmailID)
        {
            var response = new BaseResponseWithData<GetEmailsListFromDBDto>()
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
                    var email = await _emailToolService.GetEmailByIdDB(EmailID, microSoftEmailID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpPost("AddEmailsCategoryList")]
        public BaseResponseWithId<long> AddEmailsCategoryList([FromBody]AddEmailsCategoryList EmailCategoryList)
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
                    var email = _emailToolService.AddEmailsCategoryList(EmailCategoryList, validation.userID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetEmailCategoryTypeDDl")]
        public BaseResponseWithData<List<EmailCategoryTypeDDL>> GetEmailCategoryTypDDl()
        {
            var response = new BaseResponseWithData<List<EmailCategoryTypeDDL>>()
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
                    var email = _emailToolService.GetEmailCategoryTypDDl();
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetEmailUserID")]
        public async Task<BaseResponseWithId<long>> GetEmailUserID([FromHeader]string emailUserID)
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
                    var email = await _emailToolService.GetEmailUserID(emailUserID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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


        [HttpGet("GetMailsInfo")]
        public BaseResponseWithData<GetEmailInfo> GetMailsInfo()
        {
            var response = new BaseResponseWithData<GetEmailInfo>()
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
                    var email =  _emailToolService.GetMailsInfo(validation.userID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpPost("SendMicrosoftEmail")]
        public async Task<BaseResponseWithId<string>> SendMicrosoftEmail([FromForm] SendEmailMessage emailMessage)
        {
            var response = new BaseResponseWithId<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            #region validation

            //if (string.IsNullOrEmpty(emailMessage.UserID))
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = $"please enter a valid user ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            int count = 0;
            foreach (var message in emailMessage.To)
            {
                count++;
                if (string.IsNullOrEmpty(message))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = $"please enter a valid RecipientEmail at email number : {count}";
                    response.Errors.Add(error);
                    return response;
                }
            }

            int index = 0;
            foreach (var message in emailMessage.Cc)
            {
                index++;
                if (string.IsNullOrEmpty(message))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = $"please enter a valid CcEmail at email number : {index}";
                    response.Errors.Add(error);
                    return response;
                }
            }
            #endregion

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var email = await _emailToolService.SendMicrosoftEmail(emailMessage, validation.userID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("SyncAllEmails2")]
        public async Task<BaseResponseWithId<List<string>>> SyncAllEmails2()
        {
            var response = new BaseResponseWithId<List<string>> ()
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
                    var email = await _emailToolService.SyncAllEmails2(validation.userID, validation.CompanyName);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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

        [HttpGet("GetEmailAddressWithName")]
        public BaseResponseWithData<List<string>> GetEmailAddressWithName([FromHeader]string SearchKey)
        {
            var response = new BaseResponseWithData<List<string>>()
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
                    var email = _emailToolService.GetEmailAddressWithName(SearchKey);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
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
