using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class CheckClientExistanceResponse
    {
        public List<SelectDDL> ClientsList { get; set; }
        public int ClientsCount { get; set; }
        public bool IsExist { get; set; }
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
