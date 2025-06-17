using NewGarasAPI.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle
{
    public class GetVehicleBrandPerModelResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors {  get; set; }
        public List<TreeViewDto> GetVehicleBrandPerModelList {  get; set; }
    }
}
