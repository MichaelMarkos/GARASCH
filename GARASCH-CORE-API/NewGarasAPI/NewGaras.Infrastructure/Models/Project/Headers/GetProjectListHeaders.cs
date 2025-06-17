namespace NewGarasAPI.Models.Project.Headers
{
    public class GetProjectListHeaders
    {
        [FromHeader]
        public long ClientID { get; set; }

        [FromHeader]
        public bool ProjectReturn { get; set; }

        [FromHeader]
        public string ProjectName { get; set; }

        [FromHeader]
        public string SearchKey { get; set; }
    }
}
