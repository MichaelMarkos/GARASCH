using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InternalTicket
{
    public class InternalTicketDashboardSummary
    {
        public InternalTicketDashboard Day { get; set; } = new InternalTicketDashboard();
        public InternalTicketDashboard Month { get; set; } = new InternalTicketDashboard();
        public InternalTicketDashboard Year { get; set; } = new InternalTicketDashboard();
        public InternalTicketDashboard Duration { get; set; } = new InternalTicketDashboard();
    }
}
