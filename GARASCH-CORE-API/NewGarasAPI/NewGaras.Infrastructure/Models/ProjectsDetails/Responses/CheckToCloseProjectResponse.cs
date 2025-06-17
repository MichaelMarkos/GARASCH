using NewGarasAPI.Models.ProjectsDetails.UsedInResponses;

namespace NewGarasAPI.Models.ProjectsDetails.Responses
{
    public class CheckToCloseProjectResponse
    {
        [DataMember]
        public bool result { get; set; }

        [DataMember]
        public List<Error> errors { get; set; }

        [DataMember]
        public string TotalFabProgress { get; set; }

        [DataMember]
        public string TotalInsProgress { get; set; }

        [DataMember]
        public int RemainOpenFabOrders { get; set; }

        [DataMember]
        public int RemainOpenInsOrders { get; set; }

        [DataMember]
        public List<OpenOrders> FabOpenOrders { get; set; }

        [DataMember]
        public List<OpenOrders> InsOpenOrders { get; set; }
    }
}
