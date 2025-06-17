using NewGaras.Infrastructure.Models.Medical.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Entertainment
{
    public class AddEntertainmentReservationDTO
    {
        public long DoctorID { get; set; }
        public int Serial { get; set; }
        public string ReservationDate { get; set; }
        public long CustomerID { get; set; }
        public int CustomerTypeID { get; set; }
        public long ScheduleID { get; set; }
        //public long TeamID { get; set; }

        //public string Type { get; set; }
        //public long? ParentID { get; set; }
        public decimal FinalAmount { get; set; }
        public bool Active { get; set; }
        public int? CardNumber { get; set; }
        public int PaymentMethodId { get; set; }

        public List<InventoryItemAndCategoryListDTO> InventoryItemAndCategoryList { get; set; }
    }
}
