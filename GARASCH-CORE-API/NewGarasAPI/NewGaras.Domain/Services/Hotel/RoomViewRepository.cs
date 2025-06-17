
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;


namespace NewGaras.Domain.Services.Hotel
{
    public class RoomViewRepository : BaseRepository<RoomView, int>, IRoomViewRepository
    {
        protected GarasTestContext _context;
        public RoomViewRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }
    }
}
