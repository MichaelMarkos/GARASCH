using AutoMapper;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;


namespace NewGaras.Domain.Services.Hotel
{
    public class GovernorateRepository : BaseRepository<Governorate, int>, IGovernorateRepository
    {
        protected GarasTestContext _context;
        public GovernorateRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }

    }
}
