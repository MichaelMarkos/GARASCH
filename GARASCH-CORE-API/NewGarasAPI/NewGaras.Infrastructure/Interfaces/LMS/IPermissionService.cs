

namespace NewGaras.Infrastructure.Interfaces.LMS
{
    public interface IPermissionService
    {
        Task<bool> CheckUserHasPermissionManageCompetition(long UserId , int CompetitionId);
        Task<bool> CheckUserHasPermissionManageCompetitionDay(string UserId , int CompetitionDayId);
    }
}
