using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest
{
    [DataContract]
    public class AddMatrialDirectPRRequest
    {
        bool fromPODirect;
        long loginUserID;
        string requestgDate;

        List<DirectPRItem> directPRItemList;

        [DataMember]
        public List<DirectPRItem> DirectPRItemList
        {
            get
            {
                return directPRItemList;
            }

            set
            {
                directPRItemList = value;
            }
        }


        [DataMember]
        public string RequestgDate
        {
            get
            {
                return requestgDate;
            }

            set
            {
                requestgDate = value;
            }
        }


        [DataMember]
        public long LoginUserID
        {
            get
            {
                return loginUserID;
            }

            set
            {
                loginUserID = value;
            }
        }
        [DataMember]
        public bool FromPODirect
        {
            get
            {
                return fromPODirect;
            }

            set
            {
                fromPODirect = value;
            }
        }

    }

}
