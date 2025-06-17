using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class SalesAndCRMAddNewReportScreenData
    {
        public List<SelectDDL> ClientsDDL {  get; set; }
        public List<SelectDDL> ContactedByDDL { get; set; }
        public List<SelectDDL> RecievedByDDL { get; set; }
        public List<SelectDDL> ThroughDDL { get; set; }
        public List<SelectDDL> ReasonsDDL { get; set; }
        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
