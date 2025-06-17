using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetDeliveryAndShippingMethodResponse
    {
        bool result;
        List<Error> errors;
        List<DeliveryAndShippingMethodData> deliveryAndShippingMethodList;



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
        public List<DeliveryAndShippingMethodData> DeliveryAndShippingMethodList
        {
            get
            {
                return deliveryAndShippingMethodList;
            }

            set
            {
                deliveryAndShippingMethodList = value;
            }
        }
    }
}
