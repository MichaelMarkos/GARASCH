using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    [DataContract]
    public class InventortyStoreLocationListResponse
    {
        List<SelectDDL> dDLList;
        int? lastAdded;
        int? lastReleased;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<SelectDDL> DDLList
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
        public int? LastAdded
        {
            get
            {
                return lastAdded;
            }

            set
            {
                lastAdded = value;
            }
        }

        [DataMember]
        public int? LastReleased
        {
            get
            {
                return lastReleased;
            }

            set
            {
                lastReleased = value;
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


}
