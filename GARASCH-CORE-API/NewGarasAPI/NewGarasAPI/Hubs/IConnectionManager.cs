namespace NewGarasAPI.Hubs
{
    public interface IConnectionManager
    {
        void AddConnection(string username, string connectionId);
        void RemoveConnection(string connectionId);
        HashSet<string> GetConnections(string username);
        IEnumerable<string> OnlineUsers { get; }
    }
}
