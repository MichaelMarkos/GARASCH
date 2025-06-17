namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskCategoryDDLResponse
    {
        bool result;
        List<Error> errors;
        List<TaskCategoryDDLData> taskCategoryDDLlist;


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
        public List<TaskCategoryDDLData> TaskCategoryDDLlist
        {
            get
            {
                return taskCategoryDDLlist;
            }

            set
            {
                taskCategoryDDLlist = value;
            }
        }
    }
}
