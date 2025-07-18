﻿using System.Runtime.Serialization;
using NewGarasAPI.Models.Project.UsedInResponses;

namespace NewGarasAPI.Models.Project.Responses
{
    [DataContract]
    public class ProjectFabDLResponse
    {
        List<ProjectFabItemDDL> dDLList;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<ProjectFabItemDDL> DDLList
        {
            get
            {
                return dDLList;
            }

            set
            {
                dDLList = value;
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
