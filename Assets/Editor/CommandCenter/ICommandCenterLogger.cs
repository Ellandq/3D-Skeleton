namespace Editor.CommandCenter
{
    public interface ICommandCenterLogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}