using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.RoyalTent
{
    public class BaseMessageMainVariablesExcelResponse
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public List<MainVariableExcel> MainVariablesList { get; set; }
        public List<Error> Errors {  get; set; }
    }
}
