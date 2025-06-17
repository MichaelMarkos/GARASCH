using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical
{
    public class AddDoctorUserDTO
    {
        public string FirstName { get; set; }

        public string ARFirstName { get; set; }

        public string MiddleName { get; set; } = string.Empty;

        public string ARMiddleName { get; set; }

        public string LastName { get; set; }

        public string ARLastName { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public bool? Active { get; set; }

        public IFormFile? Photo { get; set; }

        //public DateTime CreationDate { get; set; }

        //public int? Age { get; set; }

        public string? Gender { get; set; }

        public string DateOfBirth { get; set; }

        public string LandLine { get; set; }

        public long? NationalityId { get; set; }

        public int? BranchID { get; set; }
        public int? DepartmentID { get; set; }
        public long? TeamId { get; set; }
        public int? JobTitleID { get; set; }

        public bool IsDeleted { get; set; } = false;

        public string Password { get; set; }
        public string ConfirmPass { get; set; }

        public string TeamsIdList { get; set; }     //list of long separated be ,
    }
}
