using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class HrUserDto
    {
        [FromForm]
        public long Id { get; set; }
        [FromForm]
        public string ARFirstName { get; set; }
        [FromForm]
        public string FirstName { get; set; }
        [FromForm]
        public bool Active { get; set; }
        [FromForm]
        public DateTime CreationDate { get; set; }
        [FromForm]
        public long ModifiedById { get; set; }
        [FromForm]
        public DateTime Modified { get; set; }
        [FromForm]
        public string ARLastName { get; set; }
        [FromForm]
        public string LastName { get; set; }
        [FromForm]
        public string? ARMiddleName { get; set; }
        [FromForm]
        public string MiddleName { get; set; } = string.Empty;
        [FromForm]
        public long CreatedById { get; set; }
        [FromForm]
        public int? JobTitleID { get; set; }
        [FromForm]
        public string DateOfBirth { get; set; }
        [FromForm]
        public string LandLine { get; set; }
        [FromForm]
        public long? NationalityId { get; set; }
        [FromForm]
        public int? MaritalStatusId { get; set; }
        [FromForm]
        public int? MilitaryStatusId { get; set; }
        [FromForm]
        public bool IsUser { get; set; }
        [FromForm]
        public long? UserID { get; set; }
        [FromForm]
        public string Email { get; set; }
        [FromForm]
        public string ImgPath { get; set; }
        [FromForm]
        public string Gender { get; set; }
        [FromForm]
        public bool? IsDeleted { get; set; }
        [FromForm]
        public string NationalID { get; set; }
        [FromForm]
        public int? PlaceOfBirthID { get; set; }
        [FromForm]
        public bool IsALive { get; set; }
        [FromForm]
        public DateTime? DateOfDeath { get; set; }
        [FromForm]
        public string Employer { get; set; }
        [FromForm]
        public long? ChurchOfPresenceID { get; set; }
        [FromForm]
        public long? BelongToChurchID { get; set; }
        [FromForm]
        public string AcademicYearName { get; set; }
        [FromForm]
        public DateTime? AcademicYearDate { get; set; }
        [FromForm]
        public IFormFile Photo { get; set; }
        //[FromForm]
        //public
    }
}
