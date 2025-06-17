namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskResponse
    {
        bool result;
        List<Error> errors;
        List<GetTaskIndex> tasksList;
        int taskCount;
        PaginationHeader paginationHeader;

        [DataMember]
        public int TaskCount
        {
            get { return taskCount; }
            set { taskCount = value; }
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

        [DataMember]
        public List<GetTaskIndex> TasksList
        {
            get
            {
                return tasksList;
            }

            set
            {
                tasksList = value;
            }
        }
        [DataMember]
        public PaginationHeader PaginationHeader
        {
            get
            {
                return paginationHeader;
            }

            set
            {
                paginationHeader = value;
            }
        }
    }
}
