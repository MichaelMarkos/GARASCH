using NewGaras.Infrastructure.Models.LMS;


namespace NewGaras.Infrastructure.Interfaces.LMS
{
    public interface IAuthLMsService
    {
        public DateTime TimeZoneEgypt();
        public  Task<List<string>> checkUserIsExistAsync(CheckUserIsExistModel model);
        public  Task<BaseResponseWithData<lectureTodayVM>> NextlectureToday(long HrUserId);
        public BaseResponseWithData<long> CovertUserIdToHrUserId(long UserId);

    }
}
