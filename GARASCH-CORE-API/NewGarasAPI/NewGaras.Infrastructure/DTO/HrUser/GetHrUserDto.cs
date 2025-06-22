using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class GetHrUserDto
    {
        public long Id { get; set; }
        public string ArfirstName { get; set; }
        public string FirstName { get; set; }
        public bool Active { get; set; }
        public string CreationDate { get; set; }
        public long ModifiedById { get; set; }
        public string Modified { get; set; }
        public string ArlastName { get; set; }
        public string LastName { get; set; }
        public string ArmiddleName { get; set; }
        public string MiddleName { get; set; }
        public long CreatedById { get; set; }
        public int? JobTitleId { get; set; }
        public string JobTitleName { get; set; }
        public string DateOfBirth { get; set; }
        public string LandLine { get; set; }
        public long? NationalityId { get; set; }
        public string NationalityName { get; set; }
        public int? MaritalStatusId { get; set; }
        public string MaritalStatusName { get; set; }
        public int? MilitaryStatusId { get; set; }
        public string MilitaryStatusName { get; set; }
        public string Email { get; set; }
        public string ImgPath { get; set; }
        public string Gender { get; set; }
        public string NationalId { get; set; }
        public int? PlaceOfBirthId { get; set; }
        public string PlaceOfBirthName { get; set; }
        public bool IsAlive { get; set; }
        public string DateOfDeath { get; set; }
        public string Employer { get; set; }
        public long? ChurchOfPresenceId { get; set; }
        public string ChurchOfPresenceName { get; set; }
        public long? BelongToChurchId { get; set; }
        public string BelongToChurchName { get; set; }
        public string AcademicYearName { get; set; }
        public string AcademicYearDate { get; set; }
        public long? UserId { get; set; }
    }
}
