using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetInventoryUOMResponse
    {
        bool result;
        List<Error> errors;
        List<InventoryUOMData> inventoryUOMDList;



        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }



        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }

        [DataMember]
        public List<InventoryUOMData> InventoryUOMDList
        {
            get
            {
                return inventoryUOMDList;
            }

            set
            {
                inventoryUOMDList = value;
            }
        }
    }
}
