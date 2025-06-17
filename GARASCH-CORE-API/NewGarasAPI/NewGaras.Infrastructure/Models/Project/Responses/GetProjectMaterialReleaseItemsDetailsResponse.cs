using System.Runtime.Serialization;
using NewGarasAPI.Models.Project.UsedInResponses;

namespace NewGarasAPI.Models.Project.Responses
{
    [DataContract]
    public class GetProjectMaterialReleaseItemsDetailsResponse
    {
        List<InventoryMatrialReleaseItemsForProject> InventoryMatrialReleaseItemsForProjectList;
        decimal? sumTotalPriceBOM;
        decimal? sumTotalPrice;

        bool result;
        List<Error> errors;

        [DataMember]
        public List<InventoryMatrialReleaseItemsForProject> ProjectReleasedItemsList
        {
            get
            {
                return InventoryMatrialReleaseItemsForProjectList;
            }

            set
            {
                InventoryMatrialReleaseItemsForProjectList = value;
            }
        }
        [DataMember]
        public decimal? SumTotalPriceBOM
        {
            get
            {
                return sumTotalPriceBOM;
            }

            set
            {
                sumTotalPriceBOM = value;
            }
        }
        [DataMember]
        public decimal? SumTotalPrice
        {
            get
            {
                return sumTotalPrice;
            }

            set
            {
                sumTotalPrice = value;
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
