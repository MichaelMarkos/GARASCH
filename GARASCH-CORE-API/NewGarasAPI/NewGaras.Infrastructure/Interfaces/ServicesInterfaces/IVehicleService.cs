using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Vehicle;
using NewGaras.Infrastructure.Models.Vehicle.filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IVehicleService
    {
        public Task<GetVehiclePerBrandResponse> GetVehicleModel([FromHeader] long VehicleBrandId);

        public BaseResponseWithId<long> AddEditVehicleModel(VehiclePerBrandData Request, long creator);

        public Task<GetVehiclePerCategoryResponse> GetVehiclePerCategory();

        public BaseResponseWithId<long> AddEditVehiclePerCategory(VehiclePerCategoryData Request, long creator);

        public Task<GetVehicleBrandPerModelResponse> GetVehicleBrandPerModel();

        public BaseResponseWithId<long> AddEditVehicleBrand(VehicleBrandData Request, long creator);

        public SelectDDLResponse GetVehicleBrandType();

        public SelectDDLResponse GetVehicleBodyType([FromHeader] long VehicleModelId);

        public SelectDDLResponse GetVehicleColorList();

        public SelectDDLResponse GetVehicleTransmissionList();

        public SelectDDLResponse GetVehicleWheelsDriveList();

        public Task<GetVehicleCategoryResponse> GetVehicleServiceCategoryDDL();
        public BaseResponseWithID AddNewClientVehicle(AddNewVehicle Request, long UserID);

        public BaseResponseWithID AddNextVehicleMaintenanceInCurrentOpenJobOrder(AddNewVehicleMaitenanceJobOrder Request, long userID);
        public GetClinetVehiclesDataForGet GetClientVehiclesDataResponse(GetClientVehiclesDataResponseFilters filters, string CompName);
        public Task<GetVehicleMaintenanceJobOrderHistoryResponse> GetVehicleMaintenanceJobOrderHistory(GetVehicleMaintenanceJobOrderHistoryFilters filters);
        public ViewVehicleMaintenanceTypesResponse ViewVehicleMaintenanceTypes(long? ClientVehicleId);
    }
}
