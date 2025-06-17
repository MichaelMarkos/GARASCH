using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetModuleResponse
    {
        bool result;
        List<Error> errors;
        List<ModuleData> moduleList;



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
        public List<ModuleData> ModuleList
        {
            get
            {
                return moduleList;
            }

            set
            {
                moduleList = value;
            }
        }

    }
}
