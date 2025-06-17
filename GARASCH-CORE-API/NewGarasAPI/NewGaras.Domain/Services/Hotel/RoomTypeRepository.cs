
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;


namespace GNewGaras.Domain.Services.Hotel
{
    public class RoomTypeRepository : BaseRepository<RoomType, int>, IRoomTypeRepository
    {
        protected GarasTestContext _context;
        public RoomTypeRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }
    }
}
