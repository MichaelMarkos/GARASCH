using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.TaskMangerProject;
using NewGaras.Infrastructure.Models.TaskMangerProject.Filters;
using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Models.TaskManager;
using NewGaras.Infrastructure.DTO.User;
using NewGaras.Infrastructure.DTO.ProjectPayment;
using NewGaras.Infrastructure.DTO.ProjectLetterOfCredit;
using NewGaras.Infrastructure.Models.ProjectLetterOfCredit;
using NewGaras.Infrastructure.Models.ProjectPayment;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface ITaskMangerProjectService
    {
        public BaseResponseWithId<long> AddTaskMangerProject(AddTaskMangerProjectDto Dto, long Creator, string CompName);

        public BaseResponseWithData<GetTaskMangerProjectDto> GetTaskMangerProject(long TaskMangerProjectID);

        public BaseResponseWithId<long> EditTaskMangerProject(EditTaskMangerProjectDto Dto, long Creator, string CompName);
        public Task<BaseResponseWithDataAndHeader<List<GetTaskMangerProjectCardsDto>>> GetTaskMangerProjectCards(TaskMangerProjectsFilters filters, long UserID);
        public BaseResponseWithId<long> AddProjectSettings(AddTaskMangerProjectSettingsDto Dto,long UserID);
        public BaseResponseWithData<GetTaskMangerProjectSettingsDto> GetProjectsSettings(long ProjectID);
        public BaseResponseWithData<List<CostTypeDDL>> ProjectCostTypes();
        public BaseResponseWithData<List<BillingTypeDDL>> BillingTypes();

        public BaseResponseWithId<long> AddUsersToProject(AddUsersToProjectDto Dto, long Creator);
        public BaseResponseWithData<GetUsersToProjectDto> GetUsersOfProjects(long projectId);
        public BaseResponseWithId<long> EditUsersOfProject(EditUsersAssignToProjectDto Dto, long Edior);
        public BaseResponseWithData<List<long>> GetMangersOfProject(long projectId);
        public BaseResponseWithData<List<MangersOfProject>> GetMangersOfProjectByTaskID(long TaskID);

        public BaseResponseWithData<decimal> GetCostsForAllTask(long ProjectID);

        public BaseResponseWithData<List<UserWithJobTitleDDL>> GetAllNormslUsersOfProjectDDl(long projectId);
        public BaseResponseWithId<long> AddProjectPaymentTerms(AddProjectPaymentTermsDto Dto, long UserID);
        public BaseResponseWithData<GetProjectPaymentTerms> GetProjectPaymentTerms(GetprojectPaymentTermsFilters filters);

        public BaseResponseWithData<List<GetPaymentTermDDL>> GetPaymentTermsDDL();

        public BaseResponseWithId<int> AddPaymentTerms(string paymentTerm);
        public BaseResponseWithId<long> EditProjectPaymentTerms(EditProjectPaymentTermsDto Dto, long UserID);
        public BaseResponseWithId<long> AddProjectLetterOfCredit(AddProjectLetterOfCreditDto Dto, long Creator);
        public BaseResponseWithDataAndHeader<List<GetProjectLetterOfCreditDto>> GetProjectLetterOfCredit(ProjectLetterOfCreditGetModel request);
        public BaseResponseWithId<long> EditProjectLetterOfCredit(EditProjectLetterOfCreditDto Dto, long Editor);
        public BaseResponseWithId<long> EditProjectLetterOfCreditStatus(long ProjectLetterOfCredit, bool status);
        public BaseResponseWithId<long> AddProjectLetterOfCreditComment(long prjectLetterOfCreditID, string comment, long Creator);
        public BaseResponseWithId<long> EditProjectLetterOfCreditComment(long CommentID, long prjectLetterOfCreditID, string comment, long Creator);
        public BaseResponseWithData<List<CostTypeDDL>> GetPaymentMethodDDl();
        public BaseResponseWithId<long> DeleteProjectSettings(long id);
        public BaseResponseWithId<long> DeleteUsersAssiginToProject(long id);
        public BaseResponseWithId<long> ArchiveProject(long id, bool IsArchived);
        public BaseResponseWithId<long> DeleteProject([FromHeader] long ProjectId);
        public BaseResponseWithData<List<CostTypeDDL>> GetLetterOfCreditTypeDDL();
        public BaseResponseWithData<string> GetProjectLetterOfCreditReport([FromHeader] ProjectLetterOfCreditGetModel request, string companyname);

        public BaseResponseWithData<List<GetProjectLetterOfCreditCommentDto>> GetProjectLetterOfCreditComment(long ProjectLetterOfCreditID);
        public BaseResponseWithId<List<long>> AddProjectLetterOfCreditCommentList(AddLetterOfCreditCommentList comments, long UserID);
        public BaseResponseWithData<string> GetProjectPaymentTermsReport([FromHeader] GetprojectPaymentTermsFilters filters, string CompanyName);

        public BaseResponseWithData<string> GetProjectsListReportExcell(int? BranchId, long? ProjectClientID, string CompName);
        public BaseResponseWithData<string> GetProjectsGeneralReportExcell(long? ProjectId, string DateFrom, string DateTo, string CompName);
    }
}
