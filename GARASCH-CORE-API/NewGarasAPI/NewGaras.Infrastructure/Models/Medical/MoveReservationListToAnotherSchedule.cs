using NewGaras.Infrastructure.Models.Medical.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical
{
    public class MoveReservationListToAnotherSchedule
    {
        public List<ReservationWithSerial> ReservationsWithSerialList { get; set; }
        public long OldScheduleID { get; set; }
        public long NewScheduleID { get; set; }
        public string NewReservationDate { get; set; }
    }
}
