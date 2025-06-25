using NewGaras.Domain.DTO.HrUser;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.DTO.VacationType;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.HR;
using NewGaras.Infrastructure.Models.HrUser;
using NewGarasAPI.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IHrUserService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public Task<BaseResponseWithData<UserEmployeeResponse>> AddHrEmployeeToUserAsync(AddHrEmployeeToUserDTO InData, long userId, string key);
        public Task<BaseResponseWithData<GetHrUserDto>> GetHrUser(long HrUserId);

        public Task<BaseResponseWithDataAndHeader<List<HrUserCardDto>>> GetAll(int CurrentPage, int NumberOfItemsPerPage, string? name,
            bool? active, int? DepId, int? jobTilteId, int? BranchId, bool? isUser, string? Email, string? mobile, bool? isDeleted
            , bool? ActiveUser);

        public BaseResponseWithDataAndHeader<HrUserListDDL> GetHrUserListDDl(int CurrentPage, int NumberOfItemsPerPage, string? searchKey, long? DoctorSpecialtyId);
        public Task<BaseResponseWithId<long>> CreateHrUser(HrUserDto NewHrUser, long UserId,string CompanyName);

        public Task<BaseResponseWithData<UserDataResponse>> EditHrEmployee(EditHrEmployeeDto NewHrData, long userId, string CompanyName, string key);

        public BaseResponseWithData<List<GetHrTeamUsersDto>> GetHrTeamUsers(long TeamId);

        public Task<BaseResponseWithData<List<HrUserJobTitleDto>>> GetAllUsersWithJobTitle(int? JobTitleId = null);

        public Task<BaseResponseWithData<List<HrUsersWithJobTitleNameImage>>> GetHrUsersWithJobTitleNameImage(int JobTitleId);
        public Task<BaseResponseWithId<long>> RetriveDeletedUser(long id);
        public BaseResponseWithData<GetAbsenceHistoryModel> GetAbsenceHistoryForUser(GetAbsenceHistoryRequest request);

        public BaseResponseWithData<GetHrUserAbsneceRequestList> GetHrUserAbsneceRequest(long HrUserId, int AbsenceTypeId);
        public BaseResponseWithData<string> GetUsersReportExcell(bool? Active, int? DeptID, long? teamID, string CompName, bool? IsUser);

        public BaseResponse AddVacationTypeForUser(AddVacationTypeForUserDto dto, long creator);

        public ActionResult<GetListOfEmployeeResponse> GetListOfEmployeeInfo(GetEmployeeInfoHeader header, long userID);

        public ActionResult<BaseResponseWithID> AddEditEmployeeAttachments(AddEditEmployeeAttachmentsRequest request, string CompanyName, long userID);
        public ActionResult<HrEmployeeAttachmentResponse> GetHREmployeeAttachment(long attachmentId);

        public ActionResult<GetEmployeeExpiredDocumentsResponse> GetEmployeeDocuments(GetEmployeeDocumentsHeader header);
        public  Task<ActionResult<BaseResponseWithID>> AddEditJobTitle(JobTitleData request, long userID);
        public Task<BaseResponseWithId<long>> EditEmployeeRoleNew(EmployeeRoleData request);
        public ActionResult<BaseResponseWithID> AddEditEmployeeInfo(EmployeeInfoDataList request, long userID, string CompName);
        public ActionResult<GetTeamUserResponse> GetTeamUser([FromHeader] long TeamID = 0);

        public ActionResult<GetTeamResponse> GetTeamsIndex();

        public Task<BaseResponseWithData<List<GetHrUserAddressDto>>> GetHrUserAddress(long HrUserId);
        public Task<List<HrUserLandLineDto>> GetHrUserLandLines(long HrUserId);
        public Task<List<HrUserSocialMediaDto>> GetHrUserSocialMedia(long HrUserId);

        public Task<ActionResult<GetUserRolegAndRoleResponse>> GetUserRoleAndGroup(int UserID = 0);

        public Task<GetUserRolegAndRoleResponse> GetUserRoleAndGroupNew(long UserID);

        public ActionResult<SelectDDLResponse> GetAbsenceTypeList();

        public ActionResult<ContractLeaveSettingListRespoonse> GetVacationTypesList();

        public ActionResult<BaseResponseWithId> AddEditVacationType(ContractLeaveSettingRequest request, long userID);

        public ActionResult<BaseResponseWithId> EditContractDetails(EditContractDetailModel request, long userID);

        public ActionResult<BaseResponseWithId> EditContractEmployeeAbsence(ContractLeaveEmployeeModel contractLeave, long userID);

        public ActionResult<GetContractDetailsResponse> GetContractDetails(long userId = 0);

        public ActionResult<JobTitlesDDLResponse> GetJobTitlesDDL();

        //public ActionResult<BaseResponseWithID> AddAttendanceData(AddEmployeesAttendanceRequest request, long userID);

        public Task<ActionResult<UserAttendanceListResponse>> GetEmployeeAttendence( GetEmployeeAttendenceHeader header);

        public Task<ActionResult<UserAttendanceListResponse>> GetUserAttendanceList( GetUserAttendanceListHeader header);

        public ActionResult<GetAbsenceDetailsResponse> GetEmployeeAbsenceDetails(long UserId = 0);

        public Task<BaseResponseWithId<long>> CreateHrUserWorker(AddHrUserWorker Worker, long UserId);

        public  Task<BaseResponse> AddAddressToHrUser(List<HrUserAddressDto> dtos);

        public Task<BaseResponse> AddAttachmentsToHrUser([FromForm] List<HrUserAttachmentDto> Attachments);

        public Task<BaseResponse> AddContactsToHrUser(HrUserContactsDto dto);

        public Task<BaseResponseWithId<long>> EditHrUserAddress(HrUserAddressDto dto);

        public Task<BaseResponseWithId<long>> EditHrUserSocialMedia(HrUserSocialMediaDto dto);

        public Task<BaseResponseWithId<long>> EditHrUserMobile(HrUserMobileDto dto);

        public Task<BaseResponseWithId<long>> EditHrUserLandLine(HrUserLandLineDto dto);

        public Task<List<HrUserMobileDto>> GetHrUserMobiles(long HrUserId);
        public Task<BaseResponseWithId<long>> AddChurchesAndPriestToHrUser(AddChurchesAndPriestToHrUserDto dto);

        public Task<BaseResponseWithData<GetHrUserContactsDto>> GetHrUserContacts(long HrUserId);
        public BaseResponseWithId<long> AddPersonStatus(AddPersonStatusDTO dto);
        public BaseResponseWithId<long> EditPersonStatus(EditPersonStatusDTO dto);
        public BaseResponseWithData<List<GetPersonStatusListDTO>> GetPersonStatusList();
        public BaseResponseWithId<long> AddHrUserStatus(AddHrUserStatusDTO dto);
        public BaseResponseWithId<long> EditHrUserStatus(EditHrUserStatusDTO dto);
        public BaseResponseWithData<List<GetHruserStatusListDTO>> GetHruserStatusList(long HrUserID);
    }
}
