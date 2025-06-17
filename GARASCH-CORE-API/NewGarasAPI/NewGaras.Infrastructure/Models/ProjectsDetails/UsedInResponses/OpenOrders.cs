namespace NewGarasAPI.Models.ProjectsDetails.UsedInResponses
{
    public class OpenOrders
    {
        [DataMember]
        public long OrderId { get; set; }

        [DataMember] 
        public string OrderName { get; set; }
    }
}
