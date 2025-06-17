
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Repositories;


namespace GarasAPP.EntityFrameworkCore.Repositories.Hotel
{
    public class AreaRepository : BaseRepository<Area, int>, IAreaRepository
    {
        protected GarasTestContext _context;
        public AreaRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }

    }

}
