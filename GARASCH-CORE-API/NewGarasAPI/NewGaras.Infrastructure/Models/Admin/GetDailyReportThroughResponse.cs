﻿using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetDailyReportThroughResponse
    {
        bool result;
        List<Error> errors;
        List<DailyReportThroughData> dailyReportThroughList;



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
        public List<DailyReportThroughData> DailyReportThroughList
        {
            get
            {
                return dailyReportThroughList;
            }

            set
            {
                dailyReportThroughList = value;
            }
        }
    }
}
