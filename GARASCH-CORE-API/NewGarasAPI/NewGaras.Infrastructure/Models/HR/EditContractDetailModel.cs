﻿using System.Runtime.Serialization;

namespace NewGarasAPI.Models.HR
{
    public class EditContractDetailModel
    {
        long id;
        int contactTypeID;
        string startDate;
        string endDate;
        int probationPeriod;
        int noticedByEmployee;
        int noticedByCompany;
        bool isAllowOverTime;
        bool isAutomatic;
        [DataMember]
        public long Id
        {
            set { id = value; }
            get { return id; }
        }
        [DataMember]
        public int ContactTypeID
        {
            get { return contactTypeID; }
            set { contactTypeID = value; }
        }
        [DataMember]
        public string StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        [DataMember]
        public string EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }
        [DataMember]
        public int ProbationPeriod
        {
            get { return probationPeriod; }
            set { probationPeriod = value; }
        }
        [DataMember]
        public int NoticedByEmployee
        {
            get { return noticedByEmployee; }
            set { noticedByEmployee = value; }
        }
        [DataMember]
        public int NoticedByCompany
        {
            get { return noticedByCompany; }
            set { noticedByCompany = value; }
        }
        [DataMember]
        public bool IsAllowOverTime
        {
            get { return isAllowOverTime; }
            set { isAllowOverTime = value; }
        }
        [DataMember]
        public bool IsAutomatic
        {
            get { return isAutomatic; }
            set { isAutomatic = value; }
        }
    }
}
