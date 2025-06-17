using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class AddEditMaintenanceTypeRequest
    {
        public long? id;
        public string name;
        public int vehicleRateId;
        public string description;
        public string comment;
        public int vehiclePriorityLevelId;
        public bool isForAllModels;
        public long bOMID;
        public List<long> vehicleMaintenanceTypeServiceSheduleCategories;
        public List<int> vehicleMaintenanceTypeForModelsStrings;
        public List<int> vehicleMaintenanceTypeForBrandsStrings;
        public int? milage;

        [DataMember]
        public long? Id
        {
            set { id = value; }
            get { return id; }
        }
        [DataMember]
        public string Name
        {
            set { name = value; }
            get { return name; }
        }
        [DataMember]
        public int VehicleRateId
        {
            set { vehicleRateId = value; }
            get { return vehicleRateId; }
        }
        [DataMember]
        public string Description
        {
            set { description = value; }
            get { return description; }
        }
        [DataMember]
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }
        [DataMember]
        public int VehiclePriorityLevelId
        {
            set { vehiclePriorityLevelId = value; }
            get { return vehiclePriorityLevelId; }
        }
        [DataMember]
        public bool IsForAllModels
        {
            get { return isForAllModels; }
            set { isForAllModels = value; }
        }
        [DataMember]
        public long BOMID
        {
            get { return bOMID; }
            set { bOMID = value; }
        }
        [DataMember]
        public List<long> VehicleMaintenanceTypeServiceSheduleCategories
        {
            get { return vehicleMaintenanceTypeServiceSheduleCategories; }
            set { vehicleMaintenanceTypeServiceSheduleCategories = value; }
        }
        [DataMember]
        public List<int> VehicleMaintenanceTypeForModelsStrings
        {
            get { return vehicleMaintenanceTypeForModelsStrings; }
            set { vehicleMaintenanceTypeForModelsStrings = value; }
        }
        [DataMember]
        public List<int> VehicleMaintenanceTypeForBrandsStrings
        {
            get { return vehicleMaintenanceTypeForBrandsStrings; }
            set { vehicleMaintenanceTypeForBrandsStrings = value; }
        }
        [DataMember]
        public int? Milage
        {
            get { return milage; }
            set { milage = value; }
        }
    }

}
