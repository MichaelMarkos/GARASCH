


using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Repositories;

namespace NewGaras.Domain.Services.Hotel
{
    public class BuildingRepository : BaseRepository<Building, int>, IBuildingRepository
    {
        protected GarasTestContext _context;
        public BuildingRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }
    }
}
