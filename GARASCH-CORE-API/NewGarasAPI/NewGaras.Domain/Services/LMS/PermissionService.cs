

using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.DBContext;

namespace NewGaras.Domain.Services.LMS
{
    public class PermissionService
    {
        protected GarasTestContext _context;

        public PermissionService(GarasTestContext context)
        {
            _context = context;
        }



        public async Task<bool> CheckUserHasPermissionManageCompetition(long HrUserId, int CompetitionId)
        {
            /*
             - Super Admin
             - Competition Creator
             - Comeptition Member Admin
             */
            bool Result = false;
             string CheckAdmin ="" ;
            //bool userIsAdminCompetition = false;
            bool userIsCompetitionOwner = false;
            bool userFromMemberAdmin = false;
            if (HrUserId > 0 && CompetitionId != 0)
            {
               // var _user = await _userManager.FindByIdAsync(UserId);
                var _user =  _context.HrUsers.Where(x=>x.Id == HrUserId).FirstOrDefault();
                if (_user is not null)
                {
                  //  userIsAdmin = await _userManager.IsInRoleAsync(_user, "admin");
                     CheckAdmin = _context.UserRoles.Where(x=>x.UserId == _user.UserId).Include("Role").Select(y=>y.Role.Name).FirstOrDefault();
                    //userIsAdminCompetition = await _userManager.IsInRoleAsync(_user, "adminCompetition");
                }
                var _competition = _context.Competitions.Where(a => a.Id == CompetitionId).Include("CompetitionMemberAdmin").FirstOrDefault();
                if (_competition is not null)
                {
                    userIsCompetitionOwner = _competition.HrUserId == HrUserId;

                    userFromMemberAdmin = _competition.CompetitionMemberAdmins?.Where(x => x.HrUserId == HrUserId).Any() ?? false;
                }
            }

            if ( CheckAdmin == "admin" || userIsCompetitionOwner || userFromMemberAdmin)
            {
                Result = true;
            }

            return Result;
        }


        public async Task<bool> CheckUserHasPermissionManageCompetitionDay(long HrUserId, int CompetitionDayId)
        {

            bool Result = false;
            bool userIsAdmin = false;
            bool userFromMemberAdmin = false;
            string CheckAdmin = "";

            if (HrUserId > 0  && CompetitionDayId != 0)
            {
                var _user = _context.HrUsers.Where(x => x.Id == HrUserId).FirstOrDefault();
                if (_user is not null)
                {

                    CheckAdmin = _context.UserRoles.Where(x => x.UserId == _user.UserId).Include("Role").Select(y => y.Role.Name).FirstOrDefault();
                }
                var _competitionId = _context.CompetitionDays.Where(x => x.Id == CompetitionDayId).Select(y => y.CompetitionId).FirstOrDefault();
                var _competition = _context.Competitions.Where(a => a.Id == _competitionId).Include("CompetitionMemberAdmin").FirstOrDefault();
                if (_competition is not null)
                {

                    userFromMemberAdmin = _competition.CompetitionMemberAdmins?.Where(x => x.HrUserId == HrUserId).Any() ?? false;
                }
            }

            if (CheckAdmin == "admin" || userFromMemberAdmin)
            {
                Result = true;
            }

            return Result;
        }
    }
}
