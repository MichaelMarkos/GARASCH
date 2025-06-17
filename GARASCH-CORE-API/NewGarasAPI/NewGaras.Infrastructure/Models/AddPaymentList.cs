using NewGaras.Infrastructure.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class AddPaymentList
    {
        public long HrUserID { get; set; }
        public int PaymentMethodID { get; set; }
        public List<AddPaymentDto> BankDetailsList { get; set; }

    }
}
