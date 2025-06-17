namespace NewGarasAPI.Models.Inventory.Requests
{
    public class AddInventoryItemMatrialReleaseRequest
    {
        long? matrialReleaseOrderId;
        long? matrialRequestOrderId;
        string requestDate;
        string requestType;
        bool? isRenew;
        bool? isFinish;
        string creationDate;


        List<MatrialReleaseItemInfo> matrialReleaseItemList;

        [DataMember]
        public List<MatrialReleaseItemInfo> MatrialReleaseItemList
        {
            get
            {
                return matrialReleaseItemList;
            }

            set
            {
                matrialReleaseItemList = value;
            }
        }

        [DataMember]
        public bool? IsFinish
        {
            get
            {
                return isFinish;
            }

            set
            {
                isFinish = value;
            }
        }
        [DataMember]
        public bool? IsRenew
        {
            get
            {
                return isRenew;
            }

            set
            {
                isRenew = value;
            }
        }
        [DataMember]
        public string RequestType
        {
            get
            {
                return requestType;
            }

            set
            {
                requestType = value;
            }
        }

        [DataMember]
        public string RequestDate
        {
            get
            {
                return requestDate;
            }

            set
            {
                requestDate = value;
            }
        }

        [DataMember]
        public long? MatrialRequestOrderId
        {
            get
            {
                return matrialRequestOrderId;
            }

            set
            {
                matrialRequestOrderId = value;
            }
        }

        [DataMember]
        public long? MatrialReleaseOrderId
        {
            get
            {
                return matrialReleaseOrderId;
            }

            set
            {
                matrialReleaseOrderId = value;
            }
        }

        [DataMember]
        public string CreationDate
        {
            get
            {
                return creationDate;
            }

            set
            {
                creationDate = value;
            }
        }
    }


}
