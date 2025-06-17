using NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    [DataContract]
    public class GetContractTypeListResponse
    {
        List<GetContractType> contracts;
        bool result;
        List<Error> errors;

        [DataMember]
        public List<GetContractType> Contracts
        {
            set { contracts = value; }
            get { return contracts; }
        }
        [DataMember]
        public bool Result
        {
            set { result = value; }
            get { return result; }
        }
        [DataMember]
        public List<Error> Errors
        {
            set { errors = value; }
            get { return errors; }
        }
    }

}
