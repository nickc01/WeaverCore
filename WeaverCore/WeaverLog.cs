using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Utilities;
using WeaverCore.Implementations;

namespace WeaverCore
{
    public static class WeaverLog
    {
        public static void Log(object obj)
        {
            Modding.Logger.Log(obj);
        }

        public static void Log(string str)
        {
            Modding.Logger.Log(str);
        }

        public static void LogError(object obj)
        {
            Modding.Logger.LogError(obj);
        }

        public static void LogError(string str)
        {
            Modding.Logger.LogError(str);
        }

        public static void LogWarning(object obj)
        {
            Modding.Logger.LogWarn(obj);
        }

        public static void LogWarning(string str)
        {
            Modding.Logger.LogWarn(str);
        }
    }
}
