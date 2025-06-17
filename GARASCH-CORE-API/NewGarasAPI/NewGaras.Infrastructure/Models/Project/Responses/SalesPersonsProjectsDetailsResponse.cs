﻿using NewGarasAPI.Models.Project.UsedInResponses;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.Responses
{
    public class SalesPersonsProjectsDetailsResponse
    {
        List<SalesPersonProjectsDetails> salesPersonsProjectsDetailsList;

        bool result;
        List<Error> errors;

        [DataMember]
        public List<SalesPersonProjectsDetails> SalesPersonsProjectsDetailsList
        {
            get
            {
                return salesPersonsProjectsDetailsList;
            }

            set
            {
                salesPersonsProjectsDetailsList = value;
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
