
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.LMS
{
    public class DurationFilterHallViewModel
    {
        [FromHeader]
        public DateTime startdate { get; set; }
        [FromHeader]
        public DateTime enddate { get; set; }
    }
}
