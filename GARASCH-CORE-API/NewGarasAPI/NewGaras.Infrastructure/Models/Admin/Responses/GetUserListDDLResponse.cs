using NewGarasAPI.Models.Admin.UsedInAdminResponses;

namespace NewGarasAPI.Models.Admin.Responses
{
    public class GetUserListDDLResponse
    {
        [DataMember]
        public bool result { get; set; }

        [DataMember]
        public List<Error> errors { get; set; }


        [DataMember]
        public List<MiniUserDDL> UsersList { get; set; }

        
    }
}
