using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class MaintenanceAndWarrentyCRM
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public int OpenWarrentyCount { get; set; }

        [DataMember]
        public int WillFinishedCount { get; set; }

        [DataMember]
        public int FinishedCount { get; set; }
    }
}
