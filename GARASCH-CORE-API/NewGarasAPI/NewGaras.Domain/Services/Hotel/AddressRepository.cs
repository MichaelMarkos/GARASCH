


using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Repositories;

namespace NewGaras.Domain.Services.Hotel
{
    public class AddressRepository : BaseRepository<ClientAddress, long>, IAddressRepository
    {
        protected GarasTestContext _context;
        public AddressRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }
    }
}
