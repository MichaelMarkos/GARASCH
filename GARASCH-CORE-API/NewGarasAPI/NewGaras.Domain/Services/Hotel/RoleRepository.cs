
using Microsoft.AspNetCore.Hosting;

using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;


namespace NewGaras.Domain.Services.Hotel
{
    public class RoleRepository : BaseRepository<Role, long>, IRoleRepository
    {
        protected GarasTestContext _context;
        public RoleRepository(GarasTestContext context/*, IOptions<JWT> jwt*/, IHostingEnvironment Environment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
        }

        public bool HasRole(string role, string id)
        {
            long.TryParse(id, out long uId);
            var userRoles = _context.UserRoles.Where(x => x.UserId == uId);
            var userRoleId = _context.Roles.FirstOrDefault(x => x.Name.Equals(role)).Id;
            if (userRoles.Any(x => x.RoleId == userRoleId))
            {
                return true;
            }
            return false;
        }
    }
}
