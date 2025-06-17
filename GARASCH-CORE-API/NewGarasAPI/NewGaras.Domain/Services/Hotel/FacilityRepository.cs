using AutoMapper;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;

namespace NewGaras.Domain.Services.Hotel
{
    public class FacilityRepository : BaseRepository<Facility, int>, IFacilityRepository
    {
        protected GarasTestContext _context;
        public FacilityRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }
    }
}
