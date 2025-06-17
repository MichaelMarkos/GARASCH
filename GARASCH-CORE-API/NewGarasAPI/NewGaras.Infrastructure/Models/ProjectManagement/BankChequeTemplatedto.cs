using System.ComponentModel.DataAnnotations;

namespace NewGarasAPI.Models.ProjectManagement
{
    public class BankChequeTemplatedto
    {
        public int? Id { get; set; }

        [Required]
        public string BankName { get; set; }

        public IFormFile Template {  get; set; }
    }
}
