﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class EditHrUserStatusDTO
    {
        public long ID { get; set; }
        public long? HrUserID { get; set; }
        public int? PersonStatusID { get; set; }
    }
}
