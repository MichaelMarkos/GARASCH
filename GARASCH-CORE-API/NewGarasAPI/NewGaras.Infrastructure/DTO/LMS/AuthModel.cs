

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class AuthModel
    {
        public string UserId { get; set; }
        public long HrUserId { get; set; }
        public bool Result { get; set; }
        public List<string> Errors { get; set; }
        //public string Message { get; set; }
        public bool? IsAuthenticated { get; set; }
        public bool Active { get; set; }
        public string Username { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public string? ImagePath { get; set; }
        public string? Phone { get; set; }
        public string? DateTimeEGP { get; set; }
        public string? LevelName { get; set; }                //new
        public int? LevelId { get; set; }                //new
        public string? SpecialDeptAndDeptName { get; set; }                //new
        public int? SpecialDeptId { get; set; }                //new
        public string? YearName { get; set; }                  //new
        public int? YearId { get; set; }                  //new
    }
}
