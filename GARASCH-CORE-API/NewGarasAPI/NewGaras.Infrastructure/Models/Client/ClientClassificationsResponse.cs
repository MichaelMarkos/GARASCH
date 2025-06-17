using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    [DataContract]
    public class ClientClassificationsResponse
    {
        List<SelectDDL> clientClassifications;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<SelectDDL> ClientClassifications
        {
            get
            {
                return clientClassifications;
            }

            set
            {
                clientClassifications = value;
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
