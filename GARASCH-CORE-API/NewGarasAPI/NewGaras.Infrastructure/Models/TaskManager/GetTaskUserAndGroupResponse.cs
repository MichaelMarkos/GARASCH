namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskUserAndGroupResponse
    {
        bool result;
        bool isTaskCreator;
        List<Error> errors;
        GetTaskData taskData;
        List<TaskUserData> taskUsers;
        List<TaskGroupData> taskGroups;
        List<TaskUserData> taskUsersAndGroups;
       
        

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
        public bool IsTaskCreator
        {
            get
            {
                return isTaskCreator;
            }

            set
            {
                isTaskCreator = value;
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
        public List<TaskUserData> TaskUsers
        {
            get
            {
                return taskUsers;
            }

            set
            {
                taskUsers = value;
            }
        }
        [DataMember]
        public List<TaskGroupData> TaskGroups
        {
            get
            {
                return taskGroups;
            }

            set
            {
                taskGroups = value;
            }
        }
        [DataMember]
        public List<TaskUserData> TaskUsersAndGroups
        {
            get
            {
                return taskUsersAndGroups;
            }

            set
            {
                taskUsersAndGroups = value;
            }
        }
        [DataMember]
        public GetTaskData TaskData
        {
            get
            {
                return taskData;
            }

            set
            {
                taskData = value;
            }
        }
    }
}
