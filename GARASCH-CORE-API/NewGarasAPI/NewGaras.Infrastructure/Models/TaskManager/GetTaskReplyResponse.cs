namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskReplyResponse
    {
        bool result;
        List<Error> errors;
        List<GetTaskReplysData> getTaskReplyList;
        PaginationHeader paginationHeader;


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
        [DataMember]
        public List<GetTaskReplysData> GetTaskReplyList
        {
            get
            {
                return getTaskReplyList;
            }

            set
            {
                getTaskReplyList = value;
            }
        }
    }
}
