namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskTypeNameResponse
    {
        bool result;
        List<Error> errors;
        List<TaskTypeData> taskTypeList;



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
        public List<TaskTypeData> TaskTypeList
        {
            get
            {
                return taskTypeList;
            }

            set
            {
                taskTypeList = value;
            }
        }
    }
}
