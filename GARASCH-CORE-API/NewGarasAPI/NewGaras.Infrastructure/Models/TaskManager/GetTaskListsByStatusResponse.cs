namespace NewGarasAPI.Models.TaskManager
{
    public class GetTaskListsByStatusResponse
    {
        bool result;
        List<Error> errors;
        List<GetTaskIndex> receivedTasksList;
        int receivedCount;
        List<GetTaskIndex> openTasksList;
        int openCount;
        List<GetTaskIndex> waitingTasksList;
        int waitingCount;
        List<GetTaskIndex> closedTasksList;
        int closedCount;
        
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
        public List<GetTaskIndex> ReceivedTasksList
        {
            get
            {
                return receivedTasksList;
            }

            set
            {
                receivedTasksList = value;
            }
        }
        [DataMember]
        public int ReceivedCount
        {
            get { return receivedCount; }
            set { receivedCount = value; }
        }
        [DataMember]
        public List<GetTaskIndex> OpenTasksList
        {
            get
            {
                return openTasksList;
            }

            set
            {
                openTasksList = value;
            }
        }
        [DataMember]
        public int OpenCount
        {
            get { return openCount; }
            set { openCount = value; }
        }
        [DataMember]
        public List<GetTaskIndex> WaitingTasksList
        {
            get
            {
                return waitingTasksList;
            }

            set
            {
                waitingTasksList = value;
            }
        }
        [DataMember]
        public int WaitingCount
        {
            get { return waitingCount; }
            set { waitingCount = value; }
        }
        [DataMember]
        public List<GetTaskIndex> ClosedTasksList
        {
            get
            {
                return closedTasksList;
            }

            set
            {
                closedTasksList = value;
            }
        }
        [DataMember]
        public int ClosedCount
        {
            get { return closedCount; }
            set { closedCount = value; }
        }

    }
}
