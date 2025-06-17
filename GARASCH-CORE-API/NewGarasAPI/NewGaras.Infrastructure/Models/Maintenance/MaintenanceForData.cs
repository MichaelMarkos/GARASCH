using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceForData
    {
        public int? ID { get; set; }
        public string ProductName { get; set; }
        public string ProjectName { get; set; }
        public string ProductBrand { get; set; }
        public string ProductFabricator { get; set; }
        public string ProductSerial { get; set; }
        public int CategoryID { get; set; }
        public int? InventoryItemID { get; set; }
        public int FabOrderID { get; set; }
        public int NumVisits { get; set; }
        public int ClientID { get; set; }
        public int VichealID { get; set; }
        public string ProjectLocation { get; set; }
        public string GeneralNote { get; set; }
        public string ProductionDate { get; set; }
        public string InstallationDate { get; set; }
        public string ContractNumber { get; set; }
        public string PRNumber { get; set; }
        public string Stops { get; set; }
        public string Capacity { get; set; }
        // More Details about Location
        public ProjectLocationDetails ProjectLocationDetails { get; set; }

    }
}
