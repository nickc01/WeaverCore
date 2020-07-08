

namespace Modding
{
    public interface ILogger
    {
        void Log(string message);

        void Log(object message);

        void LogDebug(string message);

        void LogDebug(object message);

        void LogError(string message);

        void LogError(object message);

        void LogFine(string message);

        void LogFine(object message);

        void LogWarn(string message);

        void LogWarn(object message);
    }
}