using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    [DataContract]
    public class InventortyStoreListResponse
    {
        List<InventoryStoreDDL> dDLList;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<InventoryStoreDDL> DDLList
        {
            get
            {
                return dDLList;
            }

            set
            {
                dDLList = value;
            }
        }
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

    }

    public class InventoryStoreDDL : SelectDDL
    {
        public bool NotAllowed { get; set; }
    }
}
