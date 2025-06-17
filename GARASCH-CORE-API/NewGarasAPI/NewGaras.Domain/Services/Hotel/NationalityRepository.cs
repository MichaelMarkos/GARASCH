
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;

namespace NewGaras.Domain.Services.Hotel
{
    public class NationalityRepository : BaseRepository<Nationality, int>, INationalityRepository
    {
        protected GarasTestContext _context;
        public NationalityRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }
    }
}
