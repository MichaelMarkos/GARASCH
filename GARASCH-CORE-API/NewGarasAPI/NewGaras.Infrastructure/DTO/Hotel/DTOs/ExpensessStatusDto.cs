using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class ExpensessStatusDto
    {
        public int? Id { get; set; }
        public int Qty { get; set; } = 1;
        public decimal? PriceOfUnit { get; set; }
        public decimal? TotalCost { get; set; }
        public DateTime? Date { get; set; }
        public long? CreatedBy { get; set; }
        public int ReservationId { get; set; }
        public int TypeServicesId { get; set; }
    }
    public class addexpensess 
    {
        public List<ExpensessStatusDto> dto { get; set; }
    }
}
