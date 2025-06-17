using AutoMapper;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;


namespace NewGaras.Domain.Services.Hotel
{
    public class languageeRepository : BaseRepository<Languagee, int>, IlanguageeRepository
    {
        protected GarasTestContext _context;
        public languageeRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }
    }
}
