using AutoMapper;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;


namespace NewGaras.Domain.Services.Hotel
{
    public class CountryRepository : BaseRepository<Country, int>, ICountryRepository
    {
        protected GarasTestContext _context;
        protected IUnitOfWork _unitOfWork;
        protected IMapper _mapper;
        public CountryRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }
    }
}
