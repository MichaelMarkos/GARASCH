using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Salary.SalaryDeduction
{
    public class AddDeductionTypeDto
    {
        public string Name { get; set; }

        public bool Active { get; set; }


    }
}
