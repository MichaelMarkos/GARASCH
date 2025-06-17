using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.Admin;
using NewGarasAPI.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NewGaras.Infrastructure.Models.Common
{
    public class GetInventoryItemContentTreeResponse
    {
        bool result;
        List<Error> errors;
        List<InventoryItemContent> inventoryItemContentList;
        List<TreeViewDto2> getInventoryItemCategoryList;


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
        public List<InventoryItemContent> InventoryItemContentList
        {
            get
            {
                return inventoryItemContentList;
            }

            set
            {
                inventoryItemContentList = value;
            }
        }

        [DataMember]
        public List<TreeViewDto2> GetInventoryItemCategoryList
        {
            get
            {
                return getInventoryItemCategoryList;
            }

            set
            {
                getInventoryItemCategoryList = value;
            }
        }
    }
}
