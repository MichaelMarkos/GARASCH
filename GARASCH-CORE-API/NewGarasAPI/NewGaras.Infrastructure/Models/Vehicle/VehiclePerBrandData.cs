using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Models.Vehicle
{
    public class VehiclePerBrandData
    {
        public int? ID { get; set; }
        public int? VehicleBrandID { get; set; }
        public string Name { get; set; }
        public string ModelName { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        public List<GetVehicleBodyType> VehicleBodyTypeList { get; set; }
    }
}