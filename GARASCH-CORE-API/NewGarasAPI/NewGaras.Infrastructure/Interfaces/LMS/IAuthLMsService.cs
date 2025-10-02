using NewGaras.Infrastructure.DTO.LMS;
using NewGaras.Infrastructure.Models.LMS;


namespace NewGaras.Infrastructure.Interfaces.LMS
{
    public interface IAuthLMsService
    {
        public DateTime TimeZoneEgypt();
        public  Task<List<string>> checkUserIsExistAsync(CheckUserIsExistModel model);
        public  Task<BaseResponseWithData<lectureTodayVM>> NextlectureToday(long HrUserId);
        public BaseResponseWithData<long> CovertUserIdToHrUserId(long UserId);
        public  Task<BaseResponseWithData<IEnumerable<CompetitorUserInfoDTO>>> GetUserRoleAdminCometitionListAsync();
        double Haversine(double lat1 , double lon1 , double lat2 , double lon2);

    }
}
