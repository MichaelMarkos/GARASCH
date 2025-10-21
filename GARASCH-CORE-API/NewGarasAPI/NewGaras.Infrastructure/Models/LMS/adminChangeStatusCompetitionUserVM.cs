

namespace NewGaras.Infrastructure.Models.LMS
{
    public class adminChangeStatusCompetitionUserVM
    {
        public int competitionId { get; set; }
        public List<listOfCompetitionUser> listOfCompetitionUser { get; set; }
    }

    public class listOfCompetitionUser
    {
        public long userId { get; set; }
        public string statusOfCompetition { get; set; }
    }
}