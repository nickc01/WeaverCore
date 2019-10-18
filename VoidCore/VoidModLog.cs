using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoidCore
{
    public static class VoidModLog
    {
        public static void Log(object message)
        {
            VoidCore.Instance.Log(message);
        }

        public static void Log(string message)
        {
            VoidCore.Instance.Log(message);
        }
        public static void LogError(string message)
        {
            VoidCore.Instance.LogError(message);
        }
        public static void LogError(object message)
        {
            VoidCore.Instance.LogError(message);
        }
    }
}
