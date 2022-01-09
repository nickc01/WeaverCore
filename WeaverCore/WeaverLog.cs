using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using UnityEngine;

namespace WeaverCore
{
    /// <summary>
    /// Used for logging to the console
    /// </summary>
    public static class WeaverLog
    {
        /// <summary>
        /// Logs an object to the console
        /// </summary>
        /// <param name="obj">The object to log</param>
        public static void Log(object obj)
        {
            Modding.Logger.Log(obj);
        }

        /// <summary>
        /// Logs a message to the console
        /// </summary>
        /// <param name="str">The message to log</param>
        public static void Log(string str)
        {
            Modding.Logger.Log(str);
        }

        /// <summary>
        /// Logs an object to the console as an error
        /// </summary>
        /// <param name="obj">The object to log</param>
        public static void LogError(object obj)
        {
            Modding.Logger.LogError(obj);
        }

        /// <summary>
        /// Logs an error to the console
        /// </summary>
        /// <param name="str">The error to log</param>
        public static void LogError(string str)
        {
            Modding.Logger.LogError(str);
        }

        /// <summary>
        /// Logs an object to the console as a warning
        /// </summary>
        /// <param name="obj">The object to log</param>
        public static void LogWarning(object obj)
        {
            Modding.Logger.LogWarn(obj);
        }

        /// <summary>
        /// Logs a warning to the console
        /// </summary>
        /// <param name="str">The warning to log</param>
        public static void LogWarning(string str)
        {
            Modding.Logger.LogWarn(str);
        }

        /// <summary>
        /// Logs an exception to the console
        /// </summary>
        /// <param name="e">The exception to log</param>
        public static void LogException(Exception e)
		{
#if UNITY_EDITOR
            Debug.LogException(e);
#else
            Modding.Logger.LogError(e);
#endif
        }
    }
}
