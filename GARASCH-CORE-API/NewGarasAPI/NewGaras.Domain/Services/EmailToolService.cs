using Azure;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Email;
using NewGaras.Infrastructure.DTO.EmailTool;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.EmailTool;
using NewGaras.Infrastructure.Models.EmailTool.Filters;
using NewGaras.Infrastructure.Models.EmailTool.UsInResponses;
using NewGaras.Infrastructure.Models.Mail;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using EmailAttachment = NewGaras.Infrastructure.Entities.EmailAttachment;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NewGaras.Domain.Services
{
    public class EmailToolService : IEmailToolService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private readonly IConfidentialClientApplication _app;
        private readonly string[] _scopes;
        private readonly IGraphAuthService _authService;
        private readonly HttpClient _httpClient;

        public EmailToolService( IUnitOfWork unitOfWork , IWebHostEnvironment host, IGraphAuthService authService)
        {
            
            _unitOfWork = unitOfWork;
            _host = host;
            _authService = authService;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var result = await _app.AcquireTokenForClient(_scopes).ExecuteAsync();
            return result.AccessToken;
        }

        //public EmailToolService(IUnitOfWork unitOfWork , IWebHostEnvironment host)
        //{
        //    _unitOfWork = unitOfWork;
        //    _host = host;
        //}

        #region old APIs
        //public BaseResponseWithId<List<long>> AddListOfEmails(AddListOfEmail dto, long UserID, string CompName)
        //{
        //    var response = new BaseResponseWithId<List<long>>()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };

        //    #region validation
        //    int index = 0;
        //    foreach (var item in dto.Emails)
        //    {
        //        if (string.IsNullOrWhiteSpace(item.EmailID))
        //        {
        //            response.Result = false;
        //            Error error = new Error();
        //            error.ErrorCode = "Err101";
        //            error.ErrorMSG = $"you Must Enter The EmailID at email nmber {index}";
        //            response.Errors.Add(error);
        //            return response;
        //        }
        //    }

        //    #endregion

        //    try
        //    {
        //        var IDsList = new List<long>();
        //        foreach (var newEmail in dto.Emails)
        //        {
        //            var email = new Email();
        //            email.EmailId = newEmail.EmailID;
        //            email.EmailBody = newEmail.EmailBody;
        //            email.EmailSubject = newEmail.EmailSubject;
        //            email.EmailSender = newEmail.EmailSender;
        //            email.UserId = UserID;

        //            _unitOfWork.Emails.Add(email);
        //            _unitOfWork.Complete();


        //            var emailCcList = new List<EmailCc>();
        //            foreach (var emailCc in newEmail.EmailCcList)
        //            {
        //                var newEmailCc = new EmailCc();

        //                newEmailCc.Email = emailCc.EmailCc;
        //                newEmailCc.EmailId = email.Id;
        //                newEmailCc.Active = true;

        //                emailCcList.Add(newEmailCc);
        //            }

        //            _unitOfWork.EmailCcs.AddRange(emailCcList);
        //            //_unitOfWork.Complete();

        //            var attachList = new List<EmailAttachment>();
        //            foreach (var attachment in newEmail.AttachmentList)
        //            {
        //                var projectAttachment = new EmailAttachment();

        //                var fileExtension = attachment.Content.FileName.Split('.').Last();
        //                var virtualPath = $"Attachments\\{CompName}\\Email\\{UserID}_{email.EmailId}\\";
        //                var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.Content.FileName.Trim().Replace(" ", ""));
        //                var AttachPath = Common.SaveFileIFF(virtualPath, attachment.Content, FileName, fileExtension, _host);

        //                projectAttachment.AttachmentPath = AttachPath;
        //                projectAttachment.EmailId = email.Id;

        //                attachList.Add(projectAttachment);
        //            }

        //            _unitOfWork.EmailAttachments.AddRange(attachList);
        //            _unitOfWork.Complete();

        //            IDsList.Add(email.Id);
        //        }
        //        response.ID = IDsList;
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

        //public BaseResponseWithData<GetEmailByIdDto> GetEmailById(string emailId, long? ID)
        //{
        //    var response = new BaseResponseWithData<GetEmailByIdDto>()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };

        //    if(ID == null && string.IsNullOrEmpty(emailId))
        //    {
        //        response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err101";
        //        error.ErrorMSG = $"please enter ID or EmailID";
        //        response.Errors.Add(error);
        //        return response;
        //    }
        //    try
        //    {

        //        Expression<Func<Email, bool>> creatria = (a => true);

        //        creatria = a => (
        //        ((!string.IsNullOrEmpty(emailId)) ? a.EmailId == emailId : true) &&
        //        (ID != null ? a.Id == ID : true)
        //        );

        //        var emailData = _unitOfWork.Emails.FindAll(creatria, new[] { "EmailCcs", "EmailAttachments" }).FirstOrDefault();



        //        if (emailData == null)
        //        {
        //            response.Result = false;
        //            Error error = new Error();
        //            error.ErrorCode = "Err101";
        //            error.ErrorMSG = $"No Email with this ID or EmailID";
        //            response.Errors.Add(error);
        //            return response;
        //        }

        //        var emailDataResponse = new GetEmailByIdDto();
        //        emailDataResponse.ID = emailData.Id;
        //        emailDataResponse.EmailId = emailData.EmailId;
        //        emailDataResponse.EmailBody = emailData.EmailBody;
        //        emailDataResponse.EmailSender = emailData.EmailSender;
        //        emailDataResponse.EmailSubject = emailData.EmailSubject;
        //        //emailDataResponse.UserId = emailData.UserId;

        //        var attachPathList = new List<string>();
        //        foreach (var attach in emailData.EmailAttachments)
        //        {
        //            var attachPath = Globals.baseURL + attach.AttachmentPath;
        //            attachPathList.Add(attachPath);
        //        }
        //        emailDataResponse.AttachmentList = attachPathList;

        //        var emailCcList = new List<string>();
        //        foreach (var Cc in emailData.EmailCcs)
        //        {
        //            var emailCc = Cc.Email;
        //            emailCcList.Add(emailCc);
        //        }
        //        emailDataResponse.EmailCcList = emailCcList;

        //        response.Data = emailDataResponse;
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

        //public BaseResponseWithData<List<GetEmailByIdDto>> GetAllMails(GetMailsHeaders dto)
        //{
        //    var response = new BaseResponseWithData<List<GetEmailByIdDto>>()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };

        //    try
        //    {
        //        Expression<Func<Email, bool>> creatria = (a => true);

        //        creatria = a => (
        //        ((!string.IsNullOrEmpty(dto.EmailId)) ? a.EmailId.Contains( dto.EmailId) : true) &&
        //        (dto.ID != null ? a.Id == dto.ID : true) &&
        //        ((!string.IsNullOrEmpty(dto.EmailSubject)) ? a.EmailSubject.Contains( dto.EmailSubject) : true)&&
        //        ((!string.IsNullOrEmpty(dto.EmailBody)) ? a.EmailBody.Contains(dto.EmailBody) : true)&&
        //        ((!string.IsNullOrEmpty(dto.EmailSender)) ? a.EmailSender.Contains(dto.EmailSender) : true)&&
        //        (dto.UserId != null ? a.UserId == dto.UserId : true)
        //        );

        //        var emailData = _unitOfWork.Emails.FindAllPaging(creatria,dto.currentPage,dto.numOfItemsPerPage, new[] { "EmailCcs", "EmailAttachments" });

        //        var emailsList = new List<GetEmailByIdDto>();
        //        foreach (var email in emailData)
        //        {

        //            var emailDataResponse = new GetEmailByIdDto();
        //            emailDataResponse.ID = email.Id;
        //            emailDataResponse.EmailId = email.EmailId;
        //            emailDataResponse.EmailBody = email.EmailBody;
        //            emailDataResponse.EmailSender = email.EmailSender;
        //            emailDataResponse.EmailSubject = email.EmailSubject;
        //            //emailDataResponse.UserId = email.UserId;

        //            var attachPathList = new List<string>();
        //            foreach (var attach in email.EmailAttachments)
        //            {
        //                var attachPath = Globals.baseURL + attach.AttachmentPath;
        //                attachPathList.Add(attachPath);
        //            }
        //            emailDataResponse.AttachmentList = attachPathList;

        //            var emailCcList = new List<string>();
        //            foreach (var Cc in email.EmailCcs)
        //            {
        //                var emailCc = Cc.Email;
        //                emailCcList.Add(emailCc);
        //            }
        //            emailDataResponse.EmailCcList = emailCcList;

        //            emailsList.Add(emailDataResponse);
        //        }



        //        response.Data = emailsList;
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

        #endregion
        public async Task<BaseResponseWithData<string>> TryGetEmails(string clientId, string tenantId)
        {
            var response = new BaseResponseWithData<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            try
            {
                Guid formatedClientID = new Guid();
                if (!Guid.TryParse(clientId, out formatedClientID))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "the client ID is in a wrong formate";
                    response.Errors.Add(err);
                    return response;
                }

                var result = await GetToken(clientId, tenantId);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                var microsoftResponse = await client.GetAsync("https://graph.microsoft.com/v1.0/me/messages");
                var emails = await microsoftResponse.Content.ReadAsStringAsync();

                response.Data = emails;
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

        public async Task<AuthenticationResult> GetToken(string clientId, string tenantId)
        {
            try
            {

                var clientApp = PublicClientApplicationBuilder.Create(clientId)
                    .WithTenantId(tenantId)
                    .WithRedirectUri("http://localhost")

                    .Build();
                var result = await clientApp.AcquireTokenInteractive(new[] { "Mail.Read", "Mail.Send" }).ExecuteAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public async Task<BaseResponseWithData<GetEmailByIdDto>> GetEmailById([FromHeader] string EmailUserId, [FromHeader] string messageId)
        {
            var response = new BaseResponseWithData<GetEmailByIdDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
               // Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Make the request to retrieve the specific email by message ID
               var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserId}/messages/{messageId}");

                if (responseMicrosoft.IsSuccessStatusCode)
                {
                    var email = await responseMicrosoft.Content.ReadAsStringAsync();
                    //return Ok(email);

                    var data = JsonConvert.DeserializeObject<SingleEmailMessage>(email);
                    //var emailUserId = data.value.subject;
                    var ListOfEmails = new List<GetEmailByIdDto>();


                    var emailData = new GetEmailByIdDto();

                    emailData.EmailId = data.Id;
                    emailData.EmailSubject = data.Subject;
                    emailData.EmailBody = data.Body.Content;
                    emailData.EmailSender = data.Sender.EmailAddress.Address;
                    emailData.UserId = data.ToRecipients.FirstOrDefault().EmailAddress.Address;

                    foreach (var cc in data.CcRecipients)
                    {
                        emailData.EmailCcList.Add(cc.EmailAddress.Address);
                    }

                    //Attachment to be add here
                    if (data.HasAttachments)
                    {

                    }

                    ListOfEmails.Add(emailData);

                    var finalList = new EmailBodyRsponse();
                    finalList.EmailsList = ListOfEmails;
                    response.Data = emailData;
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

        public async Task<BaseResponseWithData<EmailBodyRsponse>> GetAllEmails(SendEmailMessage emailMessage, long systemUserID)
        {
            var response = new BaseResponseWithData<EmailBodyRsponse>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var systemUserData = _unitOfWork.Users.GetById(systemUserID);

                var hrUserData = _unitOfWork.HrUsers.Find(a => a.UserId == systemUserData.Id);

                if (hrUserData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a valid HrUser ID";
                    response.Errors.Add(err);
                    return response;
                }
                var hrUserEmail = hrUserData.Email;

                //handle Attachments
                var attachments = new List<Dictionary<string, object>>();

                if (emailMessage.AttachmentsList != null)
                {
                    foreach (var file in emailMessage.AttachmentsList)
                    {
                        using var memoryStream = new MemoryStream();
                        await file.Content.CopyToAsync(memoryStream);

                        attachments.Add(new Dictionary<string, object>
                        {
                            { "@odata.type", "#microsoft.graph.fileAttachment" },
                            { "name", file.Content.FileName },
                            { "contentType", file.Content.ContentType },
                            { "contentBytes", Convert.ToBase64String(memoryStream.ToArray()) }
                        });
                    }
                }


                //Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Create the email body for Graph API
                var emailBody = new
                {
                    message = new
                    {
                        subject = emailMessage.Subject,
                        body = new
                        {
                            contentType = "Text",  // Change to "Html" if needed
                            content = emailMessage.Body
                        },
                        toRecipients = emailMessage.To.Select(email => new
                        {
                            emailAddress = new { address = email }
                        }).ToList(),
                        ccRecipients = emailMessage.Cc.Select(email => new
                        {
                            emailAddress = new { address = email }
                        }).ToList(),
                        attachments = attachments
                    },
                    saveToSentItems = "true"
                };

                // Prepare the content for the HTTP request
                var content = new StringContent(JsonConvert.SerializeObject(emailBody), Encoding.UTF8, "application/json");

                var serializedEmailContent = JsonConvert.SerializeObject(emailBody);


                //var jsonContent = JsonConvert.SerializeObject(emailContent);
                //var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                //Replace { user - id}
                //with the ID or email of the sender’s mailbox
                var responseMicroSoft = await _httpClient.PostAsync($"https://graph.microsoft.com/v1.0/users/{hrUserEmail}/sendMail", content);

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

        public async Task<BaseResponseWithId<string>> SendEmail(SendEmailMessage emailMessage, long systemUserID)      
        {
            var response = new BaseResponseWithId<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var systemUserData = _unitOfWork.Users.GetById(systemUserID);

                var hrUserData = _unitOfWork.HrUsers.Find(a => a.UserId == systemUserData.Id);

                if (hrUserData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a valid HrUser ID";
                    response.Errors.Add(err);
                    return response;
                }
                var hrUserEmail = hrUserData.Email;

                //handle Attachments
                var attachments = new List<Dictionary<string, object>>();

                if (emailMessage.AttachmentsList != null)
                {
                    foreach (var file in emailMessage.AttachmentsList)
                    {
                        using var memoryStream = new MemoryStream();
                        await file.Content.CopyToAsync(memoryStream);

                        attachments.Add(new Dictionary<string, object>
                        {
                            { "@odata.type", "#microsoft.graph.fileAttachment" },
                            { "name", file.Content.FileName },
                            { "contentType", file.Content.ContentType },
                            { "contentBytes", Convert.ToBase64String(memoryStream.ToArray()) }
                        });
                    }
                }


                //Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Create the email body for Graph API
                var emailBody = new
                {
                    message = new
                    {
                        subject = emailMessage.Subject,
                        body = new
                        {
                            contentType = "Html",  // Change to "Html" or "Text" if needed
                            content = emailMessage.Body
                        },
                        toRecipients = emailMessage.To.Select(email => new
                        {
                            emailAddress = new { address = email }
                        }).ToList(),
                        ccRecipients = emailMessage.Cc.Select(email => new
                        {
                            emailAddress = new { address = email }
                        }).ToList(),
                        attachments = attachments
                    },
                    saveToSentItems = "true"
                };

                // Prepare the content for the HTTP request
                var content = new StringContent(JsonConvert.SerializeObject(emailBody), Encoding.UTF8, "application/json");

                var serializedEmailContent = JsonConvert.SerializeObject(emailBody);


                //var jsonContent = JsonConvert.SerializeObject(emailContent);
                //var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                //Replace { user - id}
                //with the ID or email of the sender’s mailbox
                var responseMicroSoft = await _httpClient.PostAsync($"https://graph.microsoft.com/v1.0/users/{hrUserEmail}/sendMail", content);

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
        }//API to front

        public async Task<BaseResponseWithId<string>> SendMicrosoftEmail(SendEmailMessage emailMessage, long systemUserID)
        {
            var response = new BaseResponseWithId<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var systemUserData = _unitOfWork.Users.GetById(systemUserID);

                var hrUserData = _unitOfWork.HrUsers.Find(a => a.UserId == systemUserData.Id);

                if (hrUserData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a valid HrUser ID";
                    response.Errors.Add(err);
                    return response;
                }
                var hrUserEmail = hrUserData.Email;

                //handle Attachments
                var attachments = new List<Dictionary<string, object>>();

                //if (emailMessage.AttachmentsList != null)
                //{
                //    foreach (var file in emailMessage.AttachmentsList)
                //    {
                //        using var memoryStream = new MemoryStream();
                //        await file.Content.CopyToAsync(memoryStream);

                //        attachments.Add(new Dictionary<string, object>
                //        {
                //            { "@odata.type", "#microsoft.graph.fileAttachment" },
                //            { "name", file.Content.FileName },
                //            { "contentType", file.Content.ContentType },
                //            { "contentBytes", Convert.ToBase64String(memoryStream.ToArray()) }
                //        });
                //    }
                //}


                //Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Create the email body for Graph API
                var emailBody = new
                {
                    message = new
                    {
                        subject = emailMessage.Subject,
                        body = new
                        {
                            contentType = "Text",  // Change to "Html" if needed
                            content = emailMessage.Body
                        },
                        toRecipients = emailMessage.To.Select(email => new
                        {
                            emailAddress = new { address = email }
                        }).ToList(),
                        ccRecipients = emailMessage.Cc.Select(email => new
                        {
                            emailAddress = new { address = email }
                        }).ToList(),
                        attachments = attachments
                    },
                    saveToSentItems = "true"
                };

                // Prepare the content for the HTTP request
                var content = new StringContent(JsonConvert.SerializeObject(emailBody), Encoding.UTF8, "application/json");

                var serializedEmailContent = JsonConvert.SerializeObject(emailBody);


                //var jsonContent = JsonConvert.SerializeObject(emailContent);
                //var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                //Replace { user - id}
                //with the ID or email of the sender’s mailbox
                var responseMicroSoft = await _httpClient.PostAsync($"https://graph.microsoft.com/v1.0/users/{hrUserEmail}/sendMail", content);

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

        public async Task<BaseResponseWithData<EmailBodyRsponse>> GetSpamEmails(long systemUserID)
        {
            var response = new BaseResponseWithData<EmailBodyRsponse>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var systemUserData = _unitOfWork.Users.GetById(systemUserID);

                var hrUserData = _unitOfWork.HrUsers.Find(a => a.UserId == systemUserData.Id);

                if (hrUserData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a valid HrUser ID";
                    response.Errors.Add(err);
                    return response;
                }
                var hrUserEmail = hrUserData.Email;

                //Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

               // Make the request to retrieve emails from the Junk Email folder(spam folder)
                var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{hrUserEmail}/mailFolders/JunkEmail/messages");


                if (responseMicrosoft.IsSuccessStatusCode)
                {
                    var emails = await responseMicrosoft.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    var ListOfEmails = new List<GetEmailByIdDto>();

                    foreach (var email in data.Value)
                    {
                        var emailData = new GetEmailByIdDto();

                        emailData.EmailId = email.Id;
                        emailData.EmailSubject = email.Subject;
                        emailData.EmailBody = email.Body.Content;
                        emailData.EmailSender = email.Sender.EmailAddress.Address;
                        emailData.UserId = email.ToRecipients.FirstOrDefault().EmailAddress.Address;

                        foreach (var cc in email.CcRecipients)
                        {
                            emailData.EmailCcList.Add(cc.EmailAddress.Address);
                        }

                       // Attachment to be add here
                        if (email.HasAttachments)
                        {

                        }

                        ListOfEmails.Add(emailData);
                    }
                    var finalList = new EmailBodyRsponse();
                    finalList.EmailsList = ListOfEmails;
                    response.Data = finalList;
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

        public async Task<BaseResponseWithData<EmailBodyRsponse>> GetJunkEmails(long systemUserID)
        {
            var response = new BaseResponseWithData<EmailBodyRsponse>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var systemUserData = _unitOfWork.Users.GetById(systemUserID);

                var hrUserData = _unitOfWork.HrUsers.Find(a => a.UserId == systemUserData.Id);

                if (hrUserData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a valid HrUser ID";
                    response.Errors.Add(err);
                    return response;
                }
                var hrUserEmail = hrUserData.Email;
                
                //Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Make the request to retrieve emails from the Junk Email folder(spam folder)
                var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{hrUserEmail}/mailFolders/JunkEmail/messages");


                if (responseMicrosoft.IsSuccessStatusCode)
                {
                    var emails = await responseMicrosoft.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    var ListOfEmails = new List<GetEmailByIdDto>();

                    foreach (var email in data.Value)
                    {
                        var emailData = new GetEmailByIdDto();

                        emailData.EmailId = email.Id;
                        emailData.EmailSubject = email.Subject;
                        emailData.EmailBody = email.Body.Content;
                        emailData.EmailSender = email.Sender.EmailAddress.Address;
                        emailData.UserId = email.ToRecipients.FirstOrDefault().EmailAddress.Address;

                        foreach (var cc in email.CcRecipients)
                        {
                            emailData.EmailCcList.Add(cc.EmailAddress.Address);
                        }

                        //Attachment to be add here
                        if (email.HasAttachments)
                        {

                        }

                        ListOfEmails.Add(emailData);
                    }
                    var finalList = new EmailBodyRsponse();
                    finalList.EmailsList = ListOfEmails;
                    response.Data = finalList;
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


        //public async Task<IActionResult> GetEmailAttachments([FromHeader] string userId, [FromHeader] string messageId)
        //{
        //    try
        //    {
        //        // Get access token
        //        string accessToken = await _authService.GetAccessTokenAsync();

        //        // Set up HTTP client with authorization
        //        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //        // Make the request to get attachments for the specified email
        //        var response = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{userId}/messages/{messageId}/attachments");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var attachments = await response.Content.ReadAsStringAsync();
        //            //return Ok(attachments);
        //        }

        //        //return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        //    }
        //    catch (Exception ex)
        //    {
        //        //return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}


        public async Task<BaseResponseWithData<EmailAttachmentDto>> DownloadAttachmentAsIFormFile(string userId, string messageId, string CompName)
        {
            var response = new BaseResponseWithData<EmailAttachmentDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
               // Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Make the request to retrieve the specific attachment by ID
                var responseMicroSoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{userId}/messages/{messageId}/attachments");

                if (responseMicroSoft.IsSuccessStatusCode)
                {
                    var responseData = new EmailAttachmentDto();
                    responseData.UserID = userId;
                    responseData.MessageID = messageId;

                    var attachmentJson = await responseMicroSoft.Content.ReadAsStringAsync();
                    var attachmentsList = JsonConvert.DeserializeObject<EmailAttachmentResponse>(attachmentJson);

                    var attachList = new List<EmailAttachmentDtoResponses>();
                    //Extract contentBytes and convert it to a byte array
                    foreach (var attachment in attachmentsList.value)
                    {

                        var newAttachment = new EmailAttachmentDtoResponses();
                        byte[] fileBytes = Convert.FromBase64String((string)attachment.ContentBytes);

                        //Set up the IFormFile properties
                       var fileName = (string)attachment.Name;
                        var contentType = (string)attachment.ContentType;
                        var fileStream = new MemoryStream(fileBytes);

                        //Create an IFormFile instance
                        IFormFile formFile = new FormFile(fileStream, 0, fileBytes.Length, "attachment", fileName)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = contentType
                        };
                        var virtualPath = $"Attachments\\{CompName}\\EmailAttachment\\{userId}\\";

                        var savedPath = Path.Combine(_host.WebRootPath, virtualPath);
                        if (File.Exists(savedPath))
                            File.Delete(savedPath);

                        var fileExtension = formFile.FileName.Split('.').Last();
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(formFile.FileName.Trim().Replace(" ", ""));
                        var filePath = Common.SaveFileIFF(virtualPath, formFile, FileName, fileExtension, _host);

                        newAttachment.AttachmentID = attachment.Id;
                        newAttachment.LastModifiedDate = attachment.LastModifiedDateTime;
                        newAttachment.FilePath = Globals.baseURL + filePath;
                        newAttachment.MediaType = attachment.ContentType;

                        attachList.Add(newAttachment);

                    }
                    responseData.AttachList = attachList;
                    response.Data = responseData;
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

        public async Task<BaseResponseWithId<List<string>>> SyncAllEmails(long UserID, string CompName)
        {
            var response = new BaseResponseWithId<List<string>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var dubugData = new List<string>();
                var systemUserData = _unitOfWork.Users.GetById(UserID);

                var hrUserData = _unitOfWork.HrUsers.Find(a => a.UserId == systemUserData.Id);

                if (hrUserData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a valid HrUser ID";
                    response.Errors.Add(err);
                    return response;
                }
                var EmailUserID = hrUserData.Email;
                //Get access token
                string accessToken = await _authService.GetAccessTokenAsync();


                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Make the request to Microsoft Graph
                var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/Inbox/messages?$top={10000}");
                //var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/messages?$top={10000}");
                //--------------for testing-----------
                string testStatusCode = responseMicrosoft.StatusCode.ToString();


                dubugData.Add(testStatusCode);
                //------------------------------------

                var ListOfEmails = new List<Email>();

                var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                if (responseMicrosoft.IsSuccessStatusCode)
                {
                    var emails = await responseMicrosoft.Content.ReadAsStringAsync();

                    var inboxData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    int count = 0;

                    //var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                    foreach (var email in inboxData.Value)
                    {
                        count++;
                        
                        var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                        if(currentEmail == null)
                        {

                            var emailData = new Email();
                            var CcEmails = new List<string>();

                            emailData.EmailId = email.Id;
                            emailData.EmailSubject = email.Subject??" ";
                            emailData.EmailBody = email.Body.Content??" ";
                            emailData.SenderEmail = email.Sender?.EmailAddress?.Address;
                            emailData.SenderName = email.Sender?.EmailAddress?.Name;
                            emailData.UserId = UserID;
                            emailData.HasAttachment = email.HasAttachments;
                            emailData.EmailCcs = new List<EmailCc>();
                            emailData.EmailType = 1;
                            emailData.ReceivedDate = email.ReceivedDateTime;
                            emailData.CreationDate = DateTime.Now;
                            emailData.CreatedBy = UserID;
                            emailData.EmailType = 1;
                            emailData.EmailReceivers = new List<EmailReceiver>();

                            foreach (var cc in email.CcRecipients)
                            {
                                var emailCc = new EmailCc();

                                emailCc.Email = cc.EmailAddress.Address;
                                emailCc.EmailId = emailData.Id;
                                emailCc.Active = true;

                                emailData.EmailCcs.Add(emailCc);
                            }

                            foreach (var mail in email.ToRecipients)
                            {
                                var emailReceiver = new EmailReceiver();

                                emailReceiver.Email = mail.EmailAddress.Address;
                                emailReceiver.EmailId = emailData.Id;
                                emailReceiver.Active = true;

                                emailData.EmailReceivers.Add(emailReceiver);
                            }

                            //Attachment to be add here 
                            if (email.HasAttachments)
                            {
                                var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                                emailData.EmailAttachments = new List<EmailAttachment>();
                                foreach (var attach in attachList.Data.AttachList)
                                {
                                    var newAttach = new EmailAttachment();

                                    newAttach.AttachmentPath = attach.FilePath;
                                    newAttach.EmailId = emailData.Id;

                                    emailData.EmailAttachments.Add(newAttach);
                                }

                            }
                            dubugData.Add(emailData.EmailId);
                            if(email.Sender != null)
                            {
                                ListOfEmails.Add(emailData);
                            }
                        }

                    }

                   
                    var finalList = new EmailBodyRsponse();
                    //finalList.EmailsList = ListOfEmails;
                    //response.Data = finalList;

                }



                var junkMails = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/JunkEmail/messages?$top={10000}");

                //--------------for testing---------
                string testStatusCodeJunk = junkMails.StatusCode.ToString();

                dubugData.Add(testStatusCodeJunk);
                //---------------------------------

                if (junkMails.IsSuccessStatusCode)
                {
                    var emails = await junkMails.Content.ReadAsStringAsync();

                    var junkData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    int count = 0;

                    //var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                    foreach (var email in junkData.Value)
                    {
                        count++;
                        
                        var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                        if (currentEmail == null)
                        {

                            var emailData = new Email();
                            var CcEmails = new List<string>();

                            emailData.EmailId = email.Id;
                            emailData.EmailSubject = email.Subject??" ";
                            emailData.EmailBody = email.Body.Content??" ";
                            emailData.SenderEmail = email.Sender?.EmailAddress?.Address;
                            emailData.SenderName = email.Sender?.EmailAddress?.Name;
                            emailData.UserId = UserID;
                            emailData.HasAttachment = email.HasAttachments;
                            emailData.EmailCcs = new List<EmailCc>();
                            emailData.ReceivedDate = email.ReceivedDateTime;
                            emailData.CreationDate = DateTime.Now;
                            emailData.CreatedBy = UserID;
                            emailData.EmailType = 2;
                            emailData.EmailReceivers = new List<EmailReceiver>();

                            foreach (var cc in email.CcRecipients)
                            {
                                var emailCc = new EmailCc();

                                emailCc.Email = cc.EmailAddress.Address;
                                emailCc.EmailId = emailData.Id;
                                emailCc.Active = true;

                                emailData.EmailCcs.Add(emailCc);
                            }

                            foreach (var mail in email.ToRecipients)
                            {
                                var emailReceiver = new EmailReceiver();

                                emailReceiver.Email = mail.EmailAddress.Address;
                                emailReceiver.EmailId = emailData.Id;
                                emailReceiver.Active = true;

                                emailData.EmailReceivers.Add(emailReceiver);
                            }

                            //Attachment to be add here 
                            if (email.HasAttachments)
                            {
                                var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                                emailData.EmailAttachments = new List<EmailAttachment>();
                                foreach (var attach in attachList.Data.AttachList)
                                {
                                    var newAttach = new EmailAttachment();

                                    newAttach.AttachmentPath = attach.FilePath;
                                    newAttach.EmailId = emailData.Id;

                                    emailData.EmailAttachments.Add(newAttach);
                                }

                            }
                            dubugData.Add(emailData.EmailId);
                            ListOfEmails.Add(emailData);
                        }

                        
                    }


                    var finalList = new EmailBodyRsponse();
                    //finalList.EmailsList = ListOfEmails;
                    //response.Data = finalList;

                }



                #region draft
                //var draftMails = await _httpClient.GetAsync($"\"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/Drafts/messages?$top={10000}");

                //if (draftMails.IsSuccessStatusCode)
                //{
                //    var emails = await responseMicrosoft.Content.ReadAsStringAsync();

                //    var draftData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                //    int count = 0;

                //    var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                //    foreach (var email in draftData.Value)
                //    {
                //        count++;
                //        try
                //        {
                //            var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                //            if (currentEmail == null)
                //            {

                //                var emailData = new Email();
                //                var CcEmails = new List<string>();

                //                emailData.EmailId = email.Id;
                //                emailData.EmailSubject = email.Subject;
                //                emailData.EmailBody = email.Body.Content;
                //                emailData.SenderEmail = email.Sender.EmailAddress.Address;
                //                emailData.SenderName = email.Sender.EmailAddress.Name;
                //                emailData.UserId = UserID;
                //                emailData.HasAttachment = email.HasAttachments;
                //                emailData.EmailCcs = new List<EmailCc>();
                //                emailData.EmailType = 3;
                //                emailData.ReceivedDate = email.ReceivedDateTime;
                //                emailData.CreationDate = DateTime.Now;
                //                emailData.CreatedBy = UserID;

                //                foreach (var cc in email.CcRecipients)
                //                {
                //                    var emailCc = new EmailCc();

                //                    emailCc.Email = cc.EmailAddress.Address;
                //                    emailCc.EmailId = emailData.Id;
                //                    emailCc.Active = true;

                //                    emailData.EmailCcs.Add(emailCc);
                //                }

                //                //Attachment to be add here 
                //                if (email.HasAttachments)
                //                {
                //                    var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                //                    emailData.EmailAttachments = new List<EmailAttachment>();
                //                    foreach (var attach in attachList.Data.AttachList)
                //                    {
                //                        var newAttach = new EmailAttachment();

                //                        newAttach.AttachmentPath = attach.FilePath;
                //                        newAttach.EmailId = emailData.Id;

                //                        emailData.EmailAttachments.Add(newAttach);
                //                    }

                //                }

                //                ListOfEmails.Add(emailData);
                //            }

                //        }
                //        catch (Exception ex)
                //        {
                //            response.Result = false;
                //            Error err = new Error();
                //            err.ErrorCode = "E-1";
                //            err.errorMSG = "Exception : " + count + " " + ex.Message;
                //            response.Errors.Add(err);
                //            return response;
                //        }
                //    }


                //    var finalList = new EmailBodyRsponse();
                //    //finalList.EmailsList = ListOfEmails;
                //    //response.Data = finalList;

                //}
                #endregion


                var sentMails = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/SentItems/messages?$top={10000}");

                //--------------for testing---------
                string testStatusCodeSent = sentMails.StatusCode.ToString();

                dubugData.Add(testStatusCodeSent);
                //---------------------------------

                if (sentMails.IsSuccessStatusCode)
                {
                    var emails = await sentMails.Content.ReadAsStringAsync();

                    var sentData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    int count = 0;

                    //var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                    foreach (var email in sentData.Value)
                    {
                        count++;
                        
                        var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                        if (currentEmail == null)
                        {

                            var emailData = new Email();
                            var CcEmails = new List<string>();
                            var emailReceiversList = new List<string>();

                            emailData.EmailId = email.Id;
                            emailData.EmailSubject = email.Subject??" ";
                            emailData.EmailBody = email.Body.Content??" ";
                            emailData.SenderEmail = email.Sender?.EmailAddress?.Address;
                            emailData.SenderName = email.Sender?.EmailAddress?.Name;
                            emailData.UserId = UserID;
                            emailData.HasAttachment = email.HasAttachments;
                            emailData.EmailCcs = new List<EmailCc>();
                            emailData.EmailType = 5;
                            emailData.ReceivedDate = email.ReceivedDateTime;
                            emailData.CreationDate = DateTime.Now;
                            emailData.CreatedBy = UserID;
                            emailData.EmailReceivers = new List<EmailReceiver>();


                            

                            foreach (var cc in email.CcRecipients)
                            {
                                var emailCc = new EmailCc();

                                emailCc.Email = cc.EmailAddress.Address;
                                emailCc.EmailId = emailData.Id;
                                emailCc.Active = true;

                                emailData.EmailCcs.Add(emailCc);
                            }

                            foreach (var mail in email.ToRecipients)
                            {
                                var emailReceiver = new EmailReceiver();

                                emailReceiver.Email = mail.EmailAddress.Address;
                                emailReceiver.EmailId = emailData.Id;
                                emailReceiver.Active = true;

                                emailData.EmailReceivers.Add(emailReceiver);
                            }

                            //Attachment to be add here 
                            if (email.HasAttachments)
                            {
                                var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                                emailData.EmailAttachments = new List<EmailAttachment>();
                                foreach (var attach in attachList.Data.AttachList)
                                {
                                    var newAttach = new EmailAttachment();

                                    newAttach.AttachmentPath = attach.FilePath;
                                    newAttach.EmailId = emailData.Id;

                                    emailData.EmailAttachments.Add(newAttach);
                                }

                            }
                            dubugData.Add(emailData.EmailId);
                            ListOfEmails.Add(emailData);
                        }

                    }


                    var finalList = new EmailBodyRsponse();
                    //finalList.EmailsList = ListOfEmails;
                    //response.Data = finalList;

                }


                var archiveMails = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/Archive/messages?$top={10000}");

                //--------------for testing---------
                string testStatusCodeArch = archiveMails.StatusCode.ToString();

                dubugData.Add(testStatusCodeArch);
                //---------------------------------

                if (archiveMails.IsSuccessStatusCode)
                {
                    var emails = await archiveMails.Content.ReadAsStringAsync();

                    var sentData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    int count = 0;

                    //var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                    foreach (var email in sentData.Value)
                    {
                        count++;
                        
                        var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                        if (currentEmail == null)
                        {

                            var emailData = new Email();
                            var CcEmails = new List<string>();

                            emailData.EmailId = email.Id;
                            emailData.EmailSubject = email.Subject??" ";
                            emailData.EmailBody = email.Body.Content??" ";
                            emailData.SenderEmail = email.Sender?.EmailAddress?.Address;
                            emailData.SenderName = email.Sender?.EmailAddress?.Name;
                            emailData.UserId = UserID;
                            emailData.HasAttachment = email.HasAttachments;
                            emailData.EmailCcs = new List<EmailCc>();
                            emailData.EmailType = 4;
                            emailData.ReceivedDate = email.ReceivedDateTime;
                            emailData.CreationDate = DateTime.Now;
                            emailData.CreatedBy = UserID;
                            emailData.EmailReceivers = new List<EmailReceiver>();

                            foreach (var cc in email.CcRecipients)
                            {
                                var emailCc = new EmailCc();

                                emailCc.Email = cc.EmailAddress.Address;
                                emailCc.EmailId = emailData.Id;
                                emailCc.Active = true;

                                emailData.EmailCcs.Add(emailCc);
                            }

                            foreach (var mail in email.ToRecipients)
                            {
                                var emailReceiver = new EmailReceiver();

                                emailReceiver.Email = mail.EmailAddress.Address;
                                emailReceiver.EmailId = emailData.Id;
                                emailReceiver.Active = true;

                                emailData.EmailReceivers.Add(emailReceiver);
                            }

                            //Attachment to be add here 
                            if (email.HasAttachments)
                            {
                                var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                                emailData.EmailAttachments = new List<EmailAttachment>();
                                foreach (var attach in attachList.Data.AttachList)
                                {
                                    var newAttach = new EmailAttachment();

                                    newAttach.AttachmentPath = attach.FilePath;
                                    newAttach.EmailId = emailData.Id;

                                    emailData.EmailAttachments.Add(newAttach);
                                }

                            }
                            dubugData.Add(emailData.EmailId);
                            ListOfEmails.Add(emailData);
                        }

                    }


                    var finalList = new EmailBodyRsponse();
                    //finalList.EmailsList = ListOfEmails;
                    //response.Data = finalList;

                }


                var test = ListOfEmails.Select(a => a.EmailId).Distinct().ToList();
                _unitOfWork.Emails.AddRange(ListOfEmails);
                var numToBeAdded = _unitOfWork.Complete();

                string toAdd = $"number of email added to data base : {numToBeAdded}";
                dubugData.Add(toAdd);
                response.ID = dubugData;
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
        } //API to front


        public async Task<BaseResponseWithData<EmailAttachmentDto>> SaveAttachmentAsIFormFile(string userId, string messageId, string CompName)
        {
            var response = new BaseResponseWithData<EmailAttachmentDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                // Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Make the request to retrieve the specific attachment by ID
                var responseMicroSoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{userId}/messages/{messageId}/attachments");

                if (responseMicroSoft.IsSuccessStatusCode)
                {
                    var responseData = new EmailAttachmentDto();
                    responseData.UserID = userId;
                    responseData.MessageID = messageId;

                    var attachmentJson = await responseMicroSoft.Content.ReadAsStringAsync();
                    var attachmentsList = JsonConvert.DeserializeObject<EmailAttachmentResponse>(attachmentJson);

                    var attachList = new List<EmailAttachmentDtoResponses>();
                    //Extract contentBytes and convert it to a byte array
                    foreach (var attachment in attachmentsList.value)
                    {

                        var newAttachment = new EmailAttachmentDtoResponses();
                        byte[] fileBytes = Convert.FromBase64String((string)attachment.ContentBytes);

                        //Set up the IFormFile properties
                        var fileName = (string)attachment.Name;
                        var contentType = (string)attachment.ContentType;
                        var fileStream = new MemoryStream(fileBytes);

                        //Create an IFormFile instance
                        IFormFile formFile = new FormFile(fileStream, 0, fileBytes.Length, "attachment", fileName)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = contentType
                        };
                        var virtualPath = $"Attachments\\{CompName}\\EmailAttachment\\{userId}\\";

                        var savedPath = Path.Combine(_host.WebRootPath, virtualPath);
                        if (File.Exists(savedPath))
                            File.Delete(savedPath);

                        var fileExtension = formFile.FileName.Split('.').Last();
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(formFile.FileName.Trim().Replace(" ", ""));
                        var filePath = Common.SaveFileIFF(virtualPath, formFile, FileName, fileExtension, _host);

                        newAttachment.AttachmentID = attachment.Id;
                        newAttachment.LastModifiedDate = attachment.LastModifiedDateTime;
                        newAttachment.FilePath = filePath;
                        newAttachment.MediaType = attachment.ContentType;

                        attachList.Add(newAttachment);

                    }
                    responseData.AttachList = attachList;
                    response.Data = responseData;
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

        public async Task<BaseResponseWithData<EmailBodyRsponse>> GetAllEmailsForUser(long systemUserID)
        {
            var response = new BaseResponseWithData<EmailBodyRsponse>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var systemUserData = _unitOfWork.Users.GetById(systemUserID);

                var hrUserData = _unitOfWork.HrUsers.Find(a => a.UserId == systemUserData.Id);

                if (hrUserData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a valid HrUser ID";
                    response.Errors.Add(err);
                    return response;
                }
                var hrUserEmail = hrUserData.Email;
                //Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Make the request to Microsoft Graph
                var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{hrUserEmail}/messages?$top={10000}");
                if (responseMicrosoft.IsSuccessStatusCode)
                {
                    var emails = await responseMicrosoft.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    var ListOfEmails = new List<GetEmailByIdDto>();


                    foreach (var email in data.Value)
                    {
                        var emailData = new GetEmailByIdDto();
                        var CcEmails = new List<string>();

                        emailData.EmailId = email.Id;
                        emailData.EmailSubject = email.Subject;
                        emailData.EmailBody = email.Body.Content;
                        emailData.EmailSender = email.Sender.EmailAddress.Address;
                        emailData.UserId = email.ToRecipients.FirstOrDefault().EmailAddress.Address;
                        emailData.HasAttatchment = email.HasAttachments;

                        foreach (var cc in email.CcRecipients)
                        {
                            CcEmails.Add(cc.EmailAddress.Address);
                        }

                        //Attachment to be add here
                        if (email.HasAttachments)
                        {

                        }

                        emailData.EmailCcList = CcEmails;
                        ListOfEmails.Add(emailData);
                    }
                    var finalList = new EmailBodyRsponse();
                    finalList.EmailsList = ListOfEmails;
                    response.Data = finalList;

                    //var test = ListOfEmails.Where(a => a.EmailId == "AAMkADgzZmMyMDYxLWRkN2YtNDJkMC1iYzdlLTIxMTEwNGY5NjA4YQBGAAAAAAAgX7WsJxQJQaLUuPlWntjSBwCCzSsIXnj9SZYhXtqAfLNSAAAAAAEJAACCzSsIXnj9SZYhXtqAfLNSAAAfXnbaAAA=").ToList();
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

        public async Task<BaseResponseWithData<EmailBodyRsponseDTO>> GetAllEmailsForUserDB(GetAllEmailsForUserDBFilters filters, long systemUserID)
        {
            var response = new BaseResponseWithData<EmailBodyRsponseDTO>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var systemUserData = _unitOfWork.Users.GetById(systemUserID);

                if (systemUserData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a valid User ID";
                    response.Errors.Add(err);
                    return response;
                }

                if(filters.emailTypeID != null)
                {
                    var emailType = _unitOfWork.EmailTypes.GetById(filters.emailTypeID??0);
                    if(emailType == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "please enter a valid Email Type ID";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                if(filters.EmailCategoryTypeID != null)
                {
                    var emailType = _unitOfWork.EmailCategoryTypes.GetById(filters.EmailCategoryTypeID ?? 0);
                    if (emailType == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "please enter a valid Email category Type ID";
                        response.Errors.Add(err);
                        return response;
                    }
                }


                Expression<Func<Email, bool>> criteria = (a => true);
                criteria = a =>
                (
                (a.UserId == systemUserID) &&
                (!string.IsNullOrEmpty(filters.UserName) ?
                (a.SenderEmail.Contains(filters.UserName) || a.SenderName.Contains(filters.UserName) || a.EmailCcs.Any(b => b.Email.Contains(filters.UserName))) : true) &&
                (!string.IsNullOrEmpty(filters.KeyWord) ?
                (a.EmailSubject.Contains(filters.KeyWord) || a.EmailBody.Contains(filters.KeyWord)) : true) &&
                (filters.dateFrom != null ? a.ReceivedDate >= filters.dateFrom : true) &&
                (filters.dateTo != null ? a.ReceivedDate <= filters.dateTo : true) &&
                (filters.emailTypeID != null ? a.EmailType == filters.emailTypeID : true) && 
                (filters.EmailCategoryTypeID != null ? a.EmailCategories.Any(c => c.CategoryTypeId == filters.EmailCategoryTypeID) : true) 
                );

                var emailsFormDB = _unitOfWork.Emails.FindAllPaging(criteria, filters.currentPage, filters.itemsPerPage, new[] { "EmailAttachments", "EmailCcs", "EmailTypeNavigation", "User" , "EmailCategories" },(x=>x.ReceivedDate), "DESC");

                //if (!string.IsNullOrEmpty(filters.UserName))
                //{
                //    emailsFormDB = emailsFormDB.Where(a => a.SenderEmail.Contains(filters.UserName) || a.SenderName.Contains(filters.UserName) || a.EmailCcs.Any(b => b.Email.Contains(filters.UserName)));
                //}
                //if (!string.IsNullOrEmpty(filters.KeyWord))
                //{
                //    emailsFormDB = emailsFormDB.Where(a => a.EmailSubject.Contains(filters.KeyWord) || a.EmailBody.Contains(filters.KeyWord));
                //}
                //if(filters.dateFrom != null) 
                //{
                //    emailsFormDB = emailsFormDB.Where(a => a.ReceivedDate >= filters.dateFrom);
                //}
                //if(filters.dateTo != null)
                //{
                //    emailsFormDB = emailsFormDB.Where(a => a.ReceivedDate <= filters.dateTo);
                //}
                //if(filters.emailTypeID != null)
                //{
                //    emailsFormDB = emailsFormDB.Where(a => a.EmailType == filters.emailTypeID);
                //}

                var emailListDB = emailsFormDB.ToList();//.OrderBy(a => a.ReceivedDate.Date);

                var ListOfEmails = new List<GetEmailsListFromDBDto>();


                foreach (var email in emailListDB)
                {
                    var emailData = new GetEmailsListFromDBDto();
                    var CcEmails = new List<string>();
                    var AttachList = new List<string>();


                    emailData.ID = email.Id;
                    emailData.EmailId = email.EmailId;
                    emailData.EmailSubject = email.EmailSubject;
                    emailData.EmailBody = email.EmailBody;
                    emailData.EmailSender = email.SenderEmail;
                    emailData.EmailSenderName = email.SenderName;
                    emailData.UserId = email.UserId;
                    emailData.UserName = email.User.FirstName + " " + email.User.LastName;
                    emailData.HasAttatchment = email.HasAttachment;
                    emailData.RecivedDate = email.ReceivedDate.ToString();
                    emailData.EmailTypeID = email.EmailType;
                    emailData.EmailTypeName = email.EmailTypeNavigation.TypeName;
                    foreach (var cc in email.EmailCcs)
                    {
                        CcEmails.Add(cc.Email);
                    }

                    //Attachment to be add here
                    foreach (var attach in email.EmailAttachments)
                    {
                        var path = Globals.baseURL + attach.AttachmentPath;
                        AttachList.Add(path);
                    }

                    emailData.EmailCcList = CcEmails;
                    emailData.AttachmentList = AttachList;
                    ListOfEmails.Add(emailData);
                }
                var finalList = new EmailBodyRsponseDTO();
                finalList.emailsList = ListOfEmails.ToList();

                var pagedHeader = new PaginationHeader()
                {
                    TotalItems = emailsFormDB.TotalCount,
                    CurrentPage = emailsFormDB.CurrentPage,
                    TotalPages = emailsFormDB.TotalPages,
                    ItemsPerPage = filters.itemsPerPage
                };


                finalList.PaginationHeader = pagedHeader;

                response.Data = finalList;

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
        }       //API to front

        public async Task<BaseResponseWithData<GetEmailsListFromDBDto>> GetEmailByIdDB(long? EmailID, string? microSoftEmailID)
        {
            var response = new BaseResponseWithData<GetEmailsListFromDBDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                //var systemUserData = _unitOfWork.Users.GetById(systemUserID);

                if (EmailID == null && microSoftEmailID == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a at least one of Email ID or MicroDoft Email ID";
                    response.Errors.Add(err);
                    return response;
                }

                var emailsFormDB = _unitOfWork.Emails.FindAllQueryable(a => true, new[] { "EmailAttachments", "EmailCcs", "EmailTypeNavigation", "User", "EmailCategories" ,
                    "EmailCategories.CategoryType" , "EmailCategories.ModifiedByNavigation", "EmailCategories.CreatedByNavigation"});

                if (EmailID != null)
                {
                    emailsFormDB = emailsFormDB.Where(a => a.Id == EmailID);
                }
                if (!string.IsNullOrEmpty(microSoftEmailID))
                {
                    emailsFormDB = emailsFormDB.Where(a => a.EmailId == microSoftEmailID);
                }
                


                var emailDB = emailsFormDB.ToList().FirstOrDefault();
                if(emailDB == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No Email with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                var ListOfEmails = new List<GetEmailsListFromDBDto>();


                
                var emailData = new GetEmailsListFromDBDto();
                var CcEmails = new List<string>();
                var AttachList = new List<string>();
                var CategoiesList = new List<EmailCateoryListData>();

                emailData.ID = emailDB.Id;
                emailData.EmailId = emailDB.EmailId;
                emailData.EmailSubject = emailDB.EmailSubject;
                emailData.EmailBody = emailDB.EmailBody;
                emailData.EmailSender = emailDB.SenderEmail;
                emailData.EmailSenderName = emailDB.SenderName;
                emailData.UserId = emailDB.UserId;
                emailData.UserName = emailDB.User.FirstName + " " + emailDB.User.LastName;
                emailData.HasAttatchment = emailDB.HasAttachment;
                emailData.RecivedDate = emailDB.ReceivedDate.ToString();
                emailData.EmailTypeID = emailDB.EmailType;
                emailData.EmailTypeName = emailDB.EmailTypeNavigation.TypeName;
                emailData.EmailComment = emailDB.EmailComment;
                foreach (var cc in emailDB.EmailCcs)
                {
                    CcEmails.Add(cc.Email);
                }

                //Attachment to be add here
                foreach (var attach in emailDB.EmailAttachments)
                {
                    var path = Globals.baseURL + attach.AttachmentPath;
                    AttachList.Add(path);
                }
                var types = _unitOfWork.EmailTypes.GetAll();
                foreach (var cate in emailDB.EmailCategories)
                {
                    var newCategory = new EmailCateoryListData();

                    newCategory.Id = cate.Id;
                    newCategory.EmailId = cate.EmailId;
                    newCategory.CategoryTypeID = cate.CategoryTypeId;
                    newCategory.CategoryTypeName = cate.CategoryType.CategoryName;

                    //--------------------------type(project, task, Case, Team ...) --------------------
                    //1   Task
                    //2   Project
                    //3   Case
                    //4   Team
                    //5   User
                    //6   Client
                    //7   Supplier
                    //8   Department
                    newCategory.TypeID = cate.TypeId;

                    if (newCategory.CategoryTypeID == 1)
                    {
                        var task = _unitOfWork.Tasks.GetById(newCategory.TypeID);
                        if(task != null)
                        {
                            newCategory.TypeName = task.Name;
                        }
                    }

                    if (newCategory.CategoryTypeID == 2)
                    {
                        var project = _unitOfWork.Projects.Find(a => a.Id == newCategory.TypeID, new[] { "SalesOffer" });
                        if (project != null)
                        {
                            if(project.SalesOffer != null)
                            {
                                newCategory.TypeName = project.SalesOffer.ProjectName;

                            }
                        }
                    }

                    //Case not used yet ----------------------------
                    //if (newCategory.CategoryTypeID == 3)
                    //{
                    //    var task = _unitOfWork.Case.GetById(newCategory.CategoryTypeID);
                    //    if (task != null)
                    //    {
                    //        newCategory.TypeName = task.Name;
                    //    }
                    //}

                    if (newCategory.CategoryTypeID == 4)
                    {
                        var team = _unitOfWork.Teams.GetById(newCategory.TypeID);
                        if (team != null)
                        {
                            newCategory.TypeName = team.Name;
                        }
                    }

                    if (newCategory.CategoryTypeID == 5)
                    {
                        var user = _unitOfWork.Users.GetById(newCategory.TypeID);
                        if (user != null)
                        {
                            newCategory.TypeName = user.FirstName + " " + user.LastName;
                        }
                    }

                    if (newCategory.CategoryTypeID == 6)
                    {
                        var client = _unitOfWork.Clients.GetById(newCategory.TypeID);
                        if (client != null)
                        {
                            newCategory.TypeName = client.Name;
                        }
                    }

                    if (newCategory.CategoryTypeID == 7)
                    {
                        var Suppliers = _unitOfWork.Suppliers.GetById(newCategory.TypeID);
                        if (Suppliers != null)
                        {
                            newCategory.TypeName = Suppliers.Name;
                        }
                    }

                    if (newCategory.CategoryTypeID == 8)
                    {
                        var department = _unitOfWork.Departments.GetById((int)newCategory.TypeID);
                        if (department != null)
                        {
                            newCategory.TypeName = department.Name;
                        }
                    }
                    //---------------------------------------------------------------------
                    newCategory.CreatedBy = cate.CreatedBy;
                    newCategory.CreatorName = cate.CreatedByNavigation.FirstName + " " + cate.CreatedByNavigation.LastName;
                    newCategory.CreationDate = cate.CreationDate.ToString();
                    newCategory.ModifiedBy = cate.ModifiedBy;
                    newCategory.ModifiedByName = cate.ModifiedByNavigation.FirstName + " " + cate.ModifiedByNavigation.LastName;
                    newCategory.ModificationDate = cate.ModificationDate.ToString();

                    CategoiesList.Add(newCategory);
                }

                emailData.EmailCcList = CcEmails;
                emailData.AttachmentList = AttachList;
                emailData.EmailCateoryList = CategoiesList;
                
                
                response.Data = emailData;

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
        }       //API to front

        public BaseResponseWithId<long> AddEmailsCategoryList(AddEmailsCategoryList EmailCategoryList, long Creator)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                

                #region validation
                int count = 0;
                var EmailsIDs = EmailCategoryList.EmailsList.Select(a => a.EmailID).ToList();
                var emailList = _unitOfWork.Emails.FindAll(a => EmailsIDs.Contains(a.Id)).ToList();

                foreach (var email in EmailCategoryList.EmailsList)
                {
                    count++;
                    var emailData = emailList.Where(a => a.Id == email.EmailID);
                    if (emailData == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please enter a valid Email ID at record number {count}";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                var categoryTypeList = EmailCategoryList.EmailsList.SelectMany(a => a.CategoryList.Select(b => b.CategoryTypeID));
                var emailCategoryTypeListDB = _unitOfWork.EmailCategoryTypes.FindAll(a => categoryTypeList.Contains(a.Id));

                int index = 0;
                foreach (var categoryType in categoryTypeList)
                {
                    count++;
                    var emailData = emailCategoryTypeListDB.Where(a=> a.Id == categoryType);
                    if (emailData == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please enter a valid Email Category Type ID at record number {index}";
                        response.Errors.Add(err);
                        return response;
                    }
                }


                #region type Validation
                //var typeDictionary = new Dictionary<long, long>();
                //foreach (var email in EmailCategoryList.EmailsList)
                //{
                //    foreach (var item in email.CategoryList)
                //    {

                //        typeDictionary.Add(item.CategoryTypeID, item.TypeID);

                //    }
                //}

                //foreach (var dic in typeDictionary)
                //{
                //    if(dic.Key == 1)
                //    {
                //        var task = _unitOfWork.Tasks.GetById(dic.Value);
                //        if (task == null)
                //        {
                //            response.Result = false;
                //            Error err = new Error();
                //            err.ErrorCode = "E-1";
                //            err.errorMSG = $"No task with ID {dic.Value}";
                //            response.Errors.Add(err);
                //            return response;
                //        }
                //    }
                //    if (dic.Value == 2)
                //    {
                //        var project = _unitOfWork.Projects.GetById(dic.Key);
                //        if (project == null)
                //        {
                //            response.Result = false;
                //            Error err = new Error();
                //            err.ErrorCode = "E-1";
                //            err.errorMSG = $"No project with ID {dic.Value}";
                //            response.Errors.Add(err);
                //            return response;
                //        }
                //    }
                //    if (dic.Value == 3)
                //    {

                //    }
                //    if (dic.Value == 4)
                //    {

                //    }
                //    if (dic.Value == 5)
                //    {

                //    }
                //}
                #endregion
                #endregion
                var emailCategoriesList = new List<EmailCategory>();

                foreach (var email in EmailCategoryList.EmailsList)
                {
                    var currentEmail = emailList.Where(a => a.Id == email.EmailID).FirstOrDefault();
                    currentEmail.EmailComment = email.Comment;

                    foreach (var item in email.CategoryList)
                    {
                        var emailCategoryData = new EmailCategory();

                        emailCategoryData.EmailId = email.EmailID;
                        emailCategoryData.CategoryTypeId = item.CategoryTypeID;
                        emailCategoryData.TypeId = item.TypeID;
                        emailCategoryData.CreatedBy = Creator;
                        emailCategoryData.CreationDate = DateTime.Now;
                        emailCategoryData.ModifiedBy = Creator;
                        emailCategoryData.ModificationDate = DateTime.Now;

                        emailCategoriesList.Add(emailCategoryData);
                    }
                }

                var addedEmailsCategory = _unitOfWork.EmailCategories.AddRange(emailCategoriesList);
                var test = _unitOfWork.Complete();

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
        }       //API to front

        public BaseResponseWithData<List<EmailCategoryTypeDDL>> GetEmailCategoryTypDDl()
        {
            var response = new BaseResponseWithData<List<EmailCategoryTypeDDL>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {

                var categories = _unitOfWork.EmailCategoryTypes.GetAll();

                var emailCategoryDataList = new List<EmailCategoryTypeDDL>();

                foreach (var category in categories) 
                {
                    var categorydata = new EmailCategoryTypeDDL();

                    categorydata.ID = category.Id;
                    categorydata.CategoryTypeName = category.CategoryName;

                    emailCategoryDataList.Add(categorydata);
                }
                response.Data = emailCategoryDataList;
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
        }       //API to front
        public async Task<NewGaras.Domain.Models.BaseResponseWithId<long>> CreateSubscriptionAsync(string UserEmail)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var accessToken = await _authService.GetAccessTokenAsync();
                var not = _unitOfWork.Notifications.GetAll();

                // Set the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Define the subscription request payload
                var subscription = new
                {
                    changeType = "created",
                    notificationUrl = "https://a620-41-233-14-228.ngrok-free.app/test",
                    resource = $"/users/{UserEmail}/mailFolders('Inbox')/messages",
                    expirationDateTime = DateTime.UtcNow.AddMinutes(4230).ToString("o"),
                    clientState = "customClientState"
                };

                // Send the subscription request to Microsoft Graph
                var content = new StringContent(JsonConvert.SerializeObject(subscription), Encoding.UTF8, "application/json");
                var microsoftResponse = await _httpClient.PostAsync("https://graph.microsoft.com/v1.0/subscriptions", content);

                if (microsoftResponse.IsSuccessStatusCode)
                {
                    microsoftResponse.EnsureSuccessStatusCode();

                    // Read and parse the response content
                    var responseContent = await microsoftResponse.Content.ReadAsStringAsync();

                    // Deserialize the response JSON to extract the fields
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    var subscriptionId = responseData.GetProperty("id").GetString();
                    var expirationDateString = responseData.GetProperty("expirationDateTime").GetString();
                    var expirationDate = DateTime.Parse(expirationDateString);

                    var newNotification = new NotificationSubscription();
                    newNotification.SubscriptionId = subscriptionId;
                    newNotification.ExpirationDateTime = expirationDate;
                    _unitOfWork.Dispose();
                    _unitOfWork.NotificationSubscriptions.Add(newNotification);
                    _unitOfWork.Complete();

                    response.ID = newNotification.Id;
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
            // Acquire the access token

        }

        public async Task<BaseResponseWithId<long>> GetEmailUserID(string userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var accessToken = await _authService.GetAccessTokenAsync();
                

                // Set the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


                
                var microsoftResponse = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{userID}");

                if (microsoftResponse.IsSuccessStatusCode)
                {
                    microsoftResponse.EnsureSuccessStatusCode();

                    // Read and parse the response content
                    var responseContent = await microsoftResponse.Content.ReadAsStringAsync();

                    // Deserialize the response JSON to extract the fields
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    

                    
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

        public BaseResponseWithData<GetEmailInfo> GetMailsInfo(long systemUserID)
        {
            var response = new BaseResponseWithData<GetEmailInfo>() { 
                Errors = new List<Error>(),
                Result = true
            };

            var systemUserData = _unitOfWork.Users.GetById(systemUserID);

            if (systemUserData == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please enter a valid User ID";
                response.Errors.Add(err);
                return response;
            }

            try
            {
                var inboxCount = _unitOfWork.Emails.Count(a => a.UserId == systemUserID && a.EmailType == 1);
                var junkCount = _unitOfWork.Emails.Count(a => a.UserId == systemUserID && a.EmailType == 2);
                var draftCount = _unitOfWork.Emails.Count(a => a.UserId == systemUserID && a.EmailType == 3);
                var archiveCount = _unitOfWork.Emails.Count(a => a.UserId == systemUserID && a.EmailType == 4);
                var sentCount = _unitOfWork.Emails.Count(a => a.UserId == systemUserID && a.EmailType == 5);

                var userEmailData = new GetEmailInfo();

                userEmailData.NumberOfInbox = inboxCount;
                userEmailData.NumberOfJunk = junkCount;
                userEmailData.NumberOfDraft = draftCount;
                userEmailData.NumberOfArchive = archiveCount;
                userEmailData.NumberOfSent = sentCount;

                response.Data = userEmailData;

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

        public async Task<BaseResponseWithId<List<string>>> SyncAllEmails2(long UserID, string CompName)
        {
            var response = new BaseResponseWithId<List<string>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var dubugData = new List<string>();
                var systemUserData = _unitOfWork.Users.GetById(UserID);

                var hrUserData = _unitOfWork.HrUsers.Find(a => a.UserId == systemUserData.Id);

                if (hrUserData == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please enter a valid HrUser ID";
                    response.Errors.Add(err);
                    return response;
                }
                var EmailUserID = hrUserData.Email;
                //Get access token
                string accessToken = await _authService.GetAccessTokenAsync();


                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Make the request to Microsoft Graph
                //var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/Inbox/messages?$top={10000}");
                var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/messages?$top={10000}");
                //--------------for testing-----------
                string testStatusCode = responseMicrosoft.StatusCode.ToString();

                string t2 = "";
                var inboxData2 = JsonConvert.DeserializeObject<EmailResponse>(t2);


                dubugData.Add(testStatusCode);
                //------------------------------------

                var ListOfEmails = new List<Email>();

                var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                if (responseMicrosoft.IsSuccessStatusCode)
                {
                    var emails = await responseMicrosoft.Content.ReadAsStringAsync();

                    var inboxData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    int count = 0;

                    //var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                    foreach (var email in inboxData.Value)
                    {
                        count++;

                        var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                        if (currentEmail == null)
                        {

                            var emailData = new Email();
                            var CcEmails = new List<string>();

                            emailData.EmailId = email.Id;
                            emailData.EmailSubject = email.Subject;
                            emailData.EmailBody = email.Body.Content;
                            emailData.SenderEmail = email.Sender?.EmailAddress?.Address;
                            emailData.SenderName = email.Sender?.EmailAddress?.Name;
                            emailData.UserId = UserID;
                            emailData.HasAttachment = email.HasAttachments;
                            emailData.EmailCcs = new List<EmailCc>();
                            emailData.EmailType = 1;
                            emailData.ReceivedDate = email.ReceivedDateTime;
                            emailData.CreationDate = DateTime.Now;
                            emailData.CreatedBy = UserID;
                            emailData.EmailType = 1;

                            foreach (var cc in email.CcRecipients)
                            {
                                var emailCc = new EmailCc();

                                emailCc.Email = cc.EmailAddress.Address;
                                emailCc.EmailId = emailData.Id;
                                emailCc.Active = true;

                                emailData.EmailCcs.Add(emailCc);
                            }

                            //Attachment to be add here 
                            if (email.HasAttachments)
                            {
                                var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                                emailData.EmailAttachments = new List<EmailAttachment>();
                                foreach (var attach in attachList.Data.AttachList)
                                {
                                    var newAttach = new EmailAttachment();

                                    newAttach.AttachmentPath = attach.FilePath;
                                    newAttach.EmailId = emailData.Id;

                                    emailData.EmailAttachments.Add(newAttach);
                                }

                            }
                            dubugData.Add(emailData.EmailId);
                            if (email.Sender != null)
                            {
                                ListOfEmails.Add(emailData);
                            }
                        }

                    }


                    var finalList = new EmailBodyRsponse();
                    //finalList.EmailsList = ListOfEmails;
                    //response.Data = finalList;

                }



                var junkMails = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/JunkEmail/messages?$top={10000}");

                //--------------for testing---------
                string testStatusCodeJunk = junkMails.StatusCode.ToString();

                dubugData.Add(testStatusCodeJunk);
                //---------------------------------

                if (junkMails.IsSuccessStatusCode)
                {
                    var emails = await junkMails.Content.ReadAsStringAsync();

                    var junkData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    int count = 0;

                    //var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                    foreach (var email in junkData.Value)
                    {
                        count++;

                        var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                        if (currentEmail == null)
                        {

                            var emailData = new Email();
                            var CcEmails = new List<string>();

                            emailData.EmailId = email.Id;
                            emailData.EmailSubject = email.Subject;
                            emailData.EmailBody = email.Body.Content;
                            emailData.SenderEmail = email.Sender?.EmailAddress?.Address;
                            emailData.SenderName = email.Sender?.EmailAddress?.Name;
                            emailData.UserId = UserID;
                            emailData.HasAttachment = email.HasAttachments;
                            emailData.EmailCcs = new List<EmailCc>();
                            emailData.ReceivedDate = email.ReceivedDateTime;
                            emailData.CreationDate = DateTime.Now;
                            emailData.CreatedBy = UserID;
                            emailData.EmailType = 2;

                            foreach (var cc in email.CcRecipients)
                            {
                                var emailCc = new EmailCc();

                                emailCc.Email = cc.EmailAddress.Address;
                                emailCc.EmailId = emailData.Id;
                                emailCc.Active = true;

                                emailData.EmailCcs.Add(emailCc);
                            }

                            //Attachment to be add here 
                            if (email.HasAttachments)
                            {
                                var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                                emailData.EmailAttachments = new List<EmailAttachment>();
                                foreach (var attach in attachList.Data.AttachList)
                                {
                                    var newAttach = new EmailAttachment();

                                    newAttach.AttachmentPath = attach.FilePath;
                                    newAttach.EmailId = emailData.Id;

                                    emailData.EmailAttachments.Add(newAttach);
                                }

                            }
                            dubugData.Add(emailData.EmailId);
                            ListOfEmails.Add(emailData);
                        }


                    }


                    var finalList = new EmailBodyRsponse();
                    //finalList.EmailsList = ListOfEmails;
                    //response.Data = finalList;

                }



                #region draft
                //var draftMails = await _httpClient.GetAsync($"\"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/Drafts/messages?$top={10000}");

                //if (draftMails.IsSuccessStatusCode)
                //{
                //    var emails = await responseMicrosoft.Content.ReadAsStringAsync();

                //    var draftData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                //    int count = 0;

                //    var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                //    foreach (var email in draftData.Value)
                //    {
                //        count++;
                //        try
                //        {
                //            var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                //            if (currentEmail == null)
                //            {

                //                var emailData = new Email();
                //                var CcEmails = new List<string>();

                //                emailData.EmailId = email.Id;
                //                emailData.EmailSubject = email.Subject;
                //                emailData.EmailBody = email.Body.Content;
                //                emailData.SenderEmail = email.Sender.EmailAddress.Address;
                //                emailData.SenderName = email.Sender.EmailAddress.Name;
                //                emailData.UserId = UserID;
                //                emailData.HasAttachment = email.HasAttachments;
                //                emailData.EmailCcs = new List<EmailCc>();
                //                emailData.EmailType = 3;
                //                emailData.ReceivedDate = email.ReceivedDateTime;
                //                emailData.CreationDate = DateTime.Now;
                //                emailData.CreatedBy = UserID;

                //                foreach (var cc in email.CcRecipients)
                //                {
                //                    var emailCc = new EmailCc();

                //                    emailCc.Email = cc.EmailAddress.Address;
                //                    emailCc.EmailId = emailData.Id;
                //                    emailCc.Active = true;

                //                    emailData.EmailCcs.Add(emailCc);
                //                }

                //                //Attachment to be add here 
                //                if (email.HasAttachments)
                //                {
                //                    var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                //                    emailData.EmailAttachments = new List<EmailAttachment>();
                //                    foreach (var attach in attachList.Data.AttachList)
                //                    {
                //                        var newAttach = new EmailAttachment();

                //                        newAttach.AttachmentPath = attach.FilePath;
                //                        newAttach.EmailId = emailData.Id;

                //                        emailData.EmailAttachments.Add(newAttach);
                //                    }

                //                }

                //                ListOfEmails.Add(emailData);
                //            }

                //        }
                //        catch (Exception ex)
                //        {
                //            response.Result = false;
                //            Error err = new Error();
                //            err.ErrorCode = "E-1";
                //            err.errorMSG = "Exception : " + count + " " + ex.Message;
                //            response.Errors.Add(err);
                //            return response;
                //        }
                //    }


                //    var finalList = new EmailBodyRsponse();
                //    //finalList.EmailsList = ListOfEmails;
                //    //response.Data = finalList;

                //}
                #endregion


                var sentMails = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/SentItems/messages?$top={10000}");

                //--------------for testing---------
                string testStatusCodeSent = sentMails.StatusCode.ToString();

                dubugData.Add(testStatusCodeSent);
                //---------------------------------

                if (sentMails.IsSuccessStatusCode)
                {
                    var emails = await sentMails.Content.ReadAsStringAsync();

                    var sentData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    int count = 0;

                    //var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                    foreach (var email in sentData.Value)
                    {
                        count++;

                        var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                        if (currentEmail == null)
                        {

                            var emailData = new Email();
                            var CcEmails = new List<string>();

                            emailData.EmailId = email.Id;
                            emailData.EmailSubject = email.Subject;
                            emailData.EmailBody = email.Body.Content;
                            emailData.SenderEmail = email.Sender?.EmailAddress?.Address;
                            emailData.SenderName = email.Sender?.EmailAddress?.Name;
                            emailData.UserId = UserID;
                            emailData.HasAttachment = email.HasAttachments;
                            emailData.EmailCcs = new List<EmailCc>();
                            emailData.EmailType = 5;
                            emailData.ReceivedDate = email.ReceivedDateTime;
                            emailData.CreationDate = DateTime.Now;
                            emailData.CreatedBy = UserID;

                            foreach (var cc in email.CcRecipients)
                            {
                                var emailCc = new EmailCc();

                                emailCc.Email = cc.EmailAddress.Address;
                                emailCc.EmailId = emailData.Id;
                                emailCc.Active = true;

                                emailData.EmailCcs.Add(emailCc);
                            }

                            //Attachment to be add here 
                            if (email.HasAttachments)
                            {
                                var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                                emailData.EmailAttachments = new List<EmailAttachment>();
                                foreach (var attach in attachList.Data.AttachList)
                                {
                                    var newAttach = new EmailAttachment();

                                    newAttach.AttachmentPath = attach.FilePath;
                                    newAttach.EmailId = emailData.Id;

                                    emailData.EmailAttachments.Add(newAttach);
                                }

                            }
                            dubugData.Add(emailData.EmailId);
                            ListOfEmails.Add(emailData);
                        }

                    }


                    var finalList = new EmailBodyRsponse();
                    //finalList.EmailsList = ListOfEmails;
                    //response.Data = finalList;

                }


                var archiveMails = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{EmailUserID}/mailFolders/Archive/messages?$top={10000}");

                //--------------for testing---------
                string testStatusCodeArch = archiveMails.StatusCode.ToString();

                dubugData.Add(testStatusCodeArch);
                //---------------------------------

                if (archiveMails.IsSuccessStatusCode)
                {
                    var emails = await archiveMails.Content.ReadAsStringAsync();

                    var sentData = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    int count = 0;

                    //var emailList = _unitOfWork.Emails.FindAll(a => a.UserId == UserID).ToList();

                    foreach (var email in sentData.Value)
                    {
                        count++;

                        var currentEmail = emailList.Where(a => a.EmailId == email.Id).FirstOrDefault();

                        if (currentEmail == null)
                        {

                            var emailData = new Email();
                            var CcEmails = new List<string>();

                            emailData.EmailId = email.Id;
                            emailData.EmailSubject = email.Subject;
                            emailData.EmailBody = email.Body.Content;
                            emailData.SenderEmail = email.Sender?.EmailAddress?.Address;
                            emailData.SenderName = email.Sender?.EmailAddress?.Name;
                            emailData.UserId = UserID;
                            emailData.HasAttachment = email.HasAttachments;
                            emailData.EmailCcs = new List<EmailCc>();
                            emailData.EmailType = 4;
                            emailData.ReceivedDate = email.ReceivedDateTime;
                            emailData.CreationDate = DateTime.Now;
                            emailData.CreatedBy = UserID;

                            foreach (var cc in email.CcRecipients)
                            {
                                var emailCc = new EmailCc();

                                emailCc.Email = cc.EmailAddress.Address;
                                emailCc.EmailId = emailData.Id;
                                emailCc.Active = true;

                                emailData.EmailCcs.Add(emailCc);
                            }

                            //Attachment to be add here 
                            if (email.HasAttachments)
                            {
                                var attachList = await SaveAttachmentAsIFormFile(EmailUserID, email.Id, CompName);

                                emailData.EmailAttachments = new List<EmailAttachment>();
                                foreach (var attach in attachList.Data.AttachList)
                                {
                                    var newAttach = new EmailAttachment();

                                    newAttach.AttachmentPath = attach.FilePath;
                                    newAttach.EmailId = emailData.Id;

                                    emailData.EmailAttachments.Add(newAttach);
                                }

                            }
                            dubugData.Add(emailData.EmailId);
                            ListOfEmails.Add(emailData);
                        }

                    }


                    var finalList = new EmailBodyRsponse();
                    //finalList.EmailsList = ListOfEmails;
                    //response.Data = finalList;

                }


                var test = ListOfEmails.Select(a => a.EmailId).Distinct().ToList();
                _unitOfWork.Emails.AddRange(ListOfEmails);
                var numToBeAdded = _unitOfWork.Complete();

                string toAdd = $"number of email added to data base : {numToBeAdded}";
                dubugData.Add(toAdd);
                response.ID = dubugData;
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
        } //API to front

        public async Task<BaseResponseWithData<EmailBodyRsponse>> GetAllEmailsCopy(string UserID)
        {
            var response = new BaseResponseWithData<EmailBodyRsponse>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var finalList = new EmailBodyRsponse();

                var ListOfEmails = new List<GetEmailByIdDto>();
                finalList.EmailsList = ListOfEmails;
                //Get access token
                string accessToken = await _authService.GetAccessTokenAsync();

                //Set up HTTP client with authorization
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                //Make the request to Microsoft Graph
                //var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{UserID}/messages?$top={10000}");
                var responseMicrosoft = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{UserID}/mailFolders/Archive/messages?$top={10000}");
                if (responseMicrosoft.IsSuccessStatusCode)
                {
                    var emails = await responseMicrosoft.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<EmailResponse>(emails);

                    //var ListOfEmails = new List<GetEmailByIdDto>();


                    foreach (var email in data.Value)
                    {
                        var emailData = new GetEmailByIdDto();
                        var CcEmails = new List<string>();

                        emailData.EmailId = email.Id;
                        emailData.EmailSubject = email.Subject;
                        emailData.EmailBody = email.Body.Content;
                        emailData.EmailSender = email.Sender?.EmailAddress?.Address;
                        emailData.UserId = email.ToRecipients.FirstOrDefault().EmailAddress.Address;
                        emailData.HasAttatchment = email.HasAttachments;

                        foreach (var cc in email.CcRecipients)
                        {
                            CcEmails.Add(cc.EmailAddress.Address);
                        }

                        //Attachment to be add here
                        if (email.HasAttachments)
                        {

                        }

                        emailData.EmailCcList = CcEmails;
                        ListOfEmails.Add(emailData);
                    }

                    finalList.EmailsList.AddRange(ListOfEmails);
                    //response.Data = finalList;

                    //var test = ListOfEmails.Where(a => a.EmailId == "AAMkADgzZmMyMDYxLWRkN2YtNDJkMC1iYzdlLTIxMTEwNGY5NjA4YQBGAAAAAAAgX7WsJxQJQaLUuPlWntjSBwCCzSsIXnj9SZYhXtqAfLNSAAAAAAEJAACCzSsIXnj9SZYhXtqAfLNSAAAfXnbaAAA=").ToList();

                }


                var junkMails = await _httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{UserID}/mailFolders/JunkEmail/messages?$top={10000}");
                if (junkMails.IsSuccessStatusCode)
                {
                    var emailsJ = await junkMails.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<EmailResponse>(emailsJ);

                    //var ListOfEmails = new List<GetEmailByIdDto>();


                    foreach (var email in data.Value)
                    {
                        var emailData = new GetEmailByIdDto();
                        var CcEmails = new List<string>();

                        emailData.EmailId = email.Id;
                        emailData.EmailSubject = email.Subject;
                        emailData.EmailBody = email.Body.Content;
                        emailData.EmailSender = email.Sender?.EmailAddress?.Address;
                        emailData.UserId = email.ToRecipients.FirstOrDefault().EmailAddress.Address;
                        emailData.HasAttatchment = email.HasAttachments;

                        foreach (var cc in email.CcRecipients)
                        {
                            CcEmails.Add(cc.EmailAddress.Address);
                        }

                        //Attachment to be add here
                        if (email.HasAttachments)
                        {

                        }

                        emailData.EmailCcList = CcEmails;
                        ListOfEmails.Add(emailData);
                    }
                    finalList.EmailsList.AddRange(ListOfEmails);

                    //var test = ListOfEmails.Where(a => a.EmailId == "AAMkADgzZmMyMDYxLWRkN2YtNDJkMC1iYzdlLTIxMTEwNGY5NjA4YQBGAAAAAAAgX7WsJxQJQaLUuPlWntjSBwCCzSsIXnj9SZYhXtqAfLNSAAAAAAEJAACCzSsIXnj9SZYhXtqAfLNSAAAfXnbaAAA=").ToList();

                }
                response.Data = finalList;
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

        public BaseResponseWithData<List<string>> GetEmailAddressWithName(string SearchKey, int CurrentPage = 1, int NumberOfItemsPerPage = 10)
        {
            var response = new BaseResponseWithData<List<string>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                SearchKey = HttpUtility.UrlDecode(SearchKey);

                var EmailAddressList = new List<string>();

                
                var hrEmailsList = _unitOfWork.HrUsers.FindAllQueryable(a => true );
                var senderEmailsList = _unitOfWork.Emails.FindAllQueryable(a => true);
                var CcEmailsList = _unitOfWork.EmailCcs.FindAllQueryable(a => true);
                var ReceiverEmailsList = _unitOfWork.EmailReceivers.FindAllQueryable(a => true);


                if (!string.IsNullOrEmpty(SearchKey))
                {
                    hrEmailsList = hrEmailsList.Where(a => a.Email.ToLower().Contains(SearchKey.ToLower()));

                    senderEmailsList = senderEmailsList.Where(a => a.SenderEmail.ToLower().Contains(SearchKey.ToLower()));

                    CcEmailsList = CcEmailsList.Where(a => a.Email.ToLower().Contains(SearchKey.ToLower()));

                    ReceiverEmailsList = ReceiverEmailsList.Where(a => a.Email.ToLower().Contains(SearchKey.ToLower()));
                }

                var hruserEmails = hrEmailsList.Select(a => a.Email).ToList();
                EmailAddressList.AddRange(hruserEmails);


                var senderEmails = senderEmailsList.Select(a => a.SenderEmail).ToList();
                EmailAddressList.AddRange(senderEmails);

                var CcEmails = CcEmailsList.Select(a => a.Email).ToList();
                EmailAddressList.AddRange(CcEmails);

                var ReceiverEmails = CcEmailsList.Select(a => a.Email).ToList();
                EmailAddressList.AddRange(ReceiverEmails);

                EmailAddressList = EmailAddressList.Distinct().ToList();

                response.Data = EmailAddressList;
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
