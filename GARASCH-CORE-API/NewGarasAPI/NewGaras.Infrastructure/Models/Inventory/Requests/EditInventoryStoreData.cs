using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class EditInventoryStoreData
    {
        public int ID { get; set; }
        public bool Active { get; set; }
        public string StoreName { get; set; }
        public string Location { get; set; }
        public string Tel { get; set; }


        public List<EditInventoryStoreKeeperData> EditInventoryStoreKeeperData {  get; set; }
        public List<EditInventoryStoreLocationData> editInventoryStoreLocationData { get; set; }
    }
}
