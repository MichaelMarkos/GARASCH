using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Payment
{
    public class EditPaymentDto
    {
        public int? BankDetailsID { get; set; }
        public int PaymentMethodID { get; set; }
        public string BankName { get; set; }
        public string BankBranch { get; set; }
        public string AccountHolderFullName { get; set; }
        public string AccountNumber { get; set; }
        public string ExpiryDate { get; set; }
        public long HrUserID { get; set; }
    }
}
