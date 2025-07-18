﻿using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.HR
{
    public class HrEmployeeAttachmentResponse
    {
        HrEmployeeAttachment attachment;
        bool result;
        List<Error> errors;
        [DataMember]
        public HrEmployeeAttachment Attachment
        {
            get { return attachment; }
            set { attachment = value; }
        }
        [DataMember]
        public bool Result
        {
            get { return result; }
            set { result = value; }
        }
        [DataMember]
        public List<Error> Errors
        {
            get { return errors; }
            set { errors = value; }
        }
    }
}
