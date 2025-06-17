namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class FabInsMainData
    {
        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string DepName { get; set; }

        [DataMember]
        public string Date { get; set; }

        [DataMember]
        public decimal HourNum { get; set; }

        [DataMember]
        public decimal Evaluation { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public string RequestType { get; set; }

    }
}
