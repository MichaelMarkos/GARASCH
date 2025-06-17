using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.ProjectsDetails.UsedInResponses;

namespace NewGarasAPI.Models.ProjectsDetails.Responses
{
    public class GetInstallationOrdersResponse
    {
        [DataMember]
        public bool result { get; set; }

        [DataMember]
        public List<Error> errors { get; set; }

        [DataMember]
        public List<ProjectInstallationCards> projectInstallationCards { get; set; }
    }
}
