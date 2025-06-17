using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Email;
using NewGaras.Infrastructure.DTO.EmailTool;
using NewGaras.Infrastructure.Models.EmailTool;
using NewGaras.Infrastructure.Models.EmailTool.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IEmailToolService
    {
        public Task<NewGaras.Domain.Models.BaseResponseWithId<long>> CreateSubscriptionAsync(string UserEmail);

        //public BaseResponseWithId<List<long>> AddListOfEmails(AddListOfEmail dto, long UserID, string CompName);

        //public BaseResponseWithData<GetEmailByIdDto> GetEmailById(string emailId, long? ID);
        //public BaseResponseWithData<List<GetEmailByIdDto>> GetAllMails(GetMailsHeaders dto);
        public Task<BaseResponseWithData<string>> TryGetEmails(string clientId, string tenantId);


        public Task<BaseResponseWithData<GetEmailByIdDto>> GetEmailById([FromHeader] string EmailUserId, [FromHeader] string messageId);

        public Task<BaseResponseWithData<EmailBodyRsponse>> GetAllEmails(SendEmailMessage emailMessage, long systemUserID);
        public Task<BaseResponseWithId<string>> SendEmail(SendEmailMessage emailMessage, long systemUserID);

        public Task<BaseResponseWithData<EmailBodyRsponse>> GetSpamEmails(long systemUserID);
        public Task<BaseResponseWithData<EmailBodyRsponse>> GetJunkEmails(long systemUserID);

        public Task<BaseResponseWithData<EmailAttachmentDto>> DownloadAttachmentAsIFormFile(string userId, string messageId, string CompName);

        public Task<BaseResponseWithId<List<string>>> SyncAllEmails(long UserID, string CompName);

        public  Task<BaseResponseWithData<EmailBodyRsponse>> GetAllEmailsForUser(long systemUserID);
        public  Task<BaseResponseWithData<EmailBodyRsponseDTO>> GetAllEmailsForUserDB(GetAllEmailsForUserDBFilters filters, long systemUserID);
        public Task<BaseResponseWithData<GetEmailsListFromDBDto>> GetEmailByIdDB(long? EmailID, string? microSoftEmailID);
        public BaseResponseWithId<long> AddEmailsCategoryList(AddEmailsCategoryList EmailCategoryList, long Creator);
        public BaseResponseWithData<List<EmailCategoryTypeDDL>> GetEmailCategoryTypDDl();
        public Task<BaseResponseWithId<long>> GetEmailUserID(string userID);
        public BaseResponseWithData<GetEmailInfo> GetMailsInfo(long systemUserID);
        public Task<BaseResponseWithId<string>> SendMicrosoftEmail(SendEmailMessage emailMessage, long systemUserID);
        public Task<BaseResponseWithId<List<string>>> SyncAllEmails2(long UserID, string CompName);
        public Task<BaseResponseWithData<EmailBodyRsponse>> GetAllEmailsCopy(string UserID);
        public BaseResponseWithData<List<string>> GetEmailAddressWithName(string SearchKey, int CurrentPage = 1, int NumberOfItemsPerPage = 10);
    }
}
