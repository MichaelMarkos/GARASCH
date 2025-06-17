using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventortyStoreIncludeLocationListResponse
    {
        List<InventortyStoreIncludeLocation> dDLList;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<InventortyStoreIncludeLocation> DDLList
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

}
