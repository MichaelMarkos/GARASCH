using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetBundleModuleResponse
    {
        bool result;
        List<Error> errors;
        List<BundleModuleData> bundleModuleList;
        List<TreeViewDto> getBundleModuleList;



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
        public List<BundleModuleData> BundleModuleList
        {
            get
            {
                return bundleModuleList;
            }

            set
            {
                bundleModuleList = value;
            }
        }

        [DataMember]
        public List<TreeViewDto> GetBundleModuleList
        {
            get
            {
                return getBundleModuleList;
            }

            set
            {
                getBundleModuleList = value;
            }
        }
    }
}
