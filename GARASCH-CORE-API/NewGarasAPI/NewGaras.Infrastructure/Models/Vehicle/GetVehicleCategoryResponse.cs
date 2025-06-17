using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle
{
    public class GetVehicleCategoryResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors {  get; set; }
        public List<VehiclePerCategoryData> VehicleCategoryList2 { get; set; }
        public List<VehiclePerCategoryData> VehicleCategoryList {  get; set; }
    }
}
