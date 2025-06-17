using NewGarasAPI.Models.Project.UsedInResponses;

namespace NewGarasAPI.Models.Project.Responses
{
    public class FabInsMainReportsResponse
    {
        [DataMember]
        public bool result { get; set; }

        [DataMember]
        public List<Error> errors { get; set; }

        [DataMember]
        public List<FabInsMainData> FabInsMainData { get; set; }

        [DataMember]
        public PaginationHeader paginationHeader { get; set; }

        [DataMember]
        public decimal SumOfHours { get; set; }

        [DataMember]
        public decimal AverageOfEvaluation { get; set; }

    }
}
