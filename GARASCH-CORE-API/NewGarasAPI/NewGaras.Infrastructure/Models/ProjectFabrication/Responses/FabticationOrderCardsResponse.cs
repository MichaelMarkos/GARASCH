using NewGarasAPI.Models.ProjectFabrication.UsedInResponses;

namespace NewGarasAPI.Models.ProjectFabrication.Responses
{
    [DataContract]
    public class FabticationOrderCardsResponse
    {
        List<MiniFabticationOrderCard> fabticationOrderCards;

        bool result;
        List<Error> errors;

        [DataMember]
        public List<MiniFabticationOrderCard> FabticationOrderCards
        {
            get
            {
                return fabticationOrderCards;
            }

            set
            {
                fabticationOrderCards = value;
            }
        }

        [DataMember]
        public bool Result
        {
            get
            {
                return result;
                
            }

            set
            {
                result = value;
            }
        }
        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }
    }
}
