using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoidCore
{
    internal static class ModLog
    {
        public static void Log(object message)
        {
            Modding.Logger.Log(message);
        }

        public static void Log(string message)
        {
            Modding.Logger.Log(message);
        }
        public static void LogError(string message)
        {
            Modding.Logger.LogError(message);
        }
        public static void LogError(object message)
        {
            Modding.Logger.LogError(message);
        }
    }
}
