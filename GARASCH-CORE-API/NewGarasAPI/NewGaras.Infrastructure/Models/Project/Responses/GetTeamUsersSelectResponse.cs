using NewGaras.Infrastructure.Models.Project.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.Responses
{
    public class GetTeamUsersSelectResponse
    {
        bool result;

        List<Error> errors;
        List<TeamUsersSelectResponseData> teamUsersSelectResponseDataList;



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
        public List<TeamUsersSelectResponseData> TeamUsersSelectResponseDataList
        {
            get
            {
                return teamUsersSelectResponseDataList;
            }

            set
            {
                teamUsersSelectResponseDataList = value;
            }
        }


    }

}
