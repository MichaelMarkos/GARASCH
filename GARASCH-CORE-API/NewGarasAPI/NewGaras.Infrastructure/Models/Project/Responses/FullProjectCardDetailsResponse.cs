using System.Runtime.Serialization;
using NewGarasAPI.Models.Project.UsedInResponses;

namespace NewGarasAPI.Models.Project.Responses
{
    [DataContract]
    public class FullProjectCardDetailsResponse
    {
        ProjectCard projectCard;

        bool result;
        List<Error> errors;

        [DataMember]
        public ProjectCard ProjectCard
        {
            get
            {
                return projectCard;
            }

            set
            {
                projectCard = value;
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
