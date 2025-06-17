
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;

namespace NewGaras.Domain.Services.Hotel
{
    public class ExpensessStatusRepository : BaseRepository<ExpensessStatus, int>, IExpensessStatusRepository
    {
        protected GarasTestContext _context;
        public ExpensessStatusRepository(GarasTestContext context) : base(context)
        {
            _context = context;
        }
    }
}
