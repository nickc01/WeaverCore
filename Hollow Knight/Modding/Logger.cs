using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Modding
{
	public static class Logger
	{
		public static void Log(string message, LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Fine:
					Debug.Log(message);
					break;
				case LogLevel.Debug:
					Debug.Log(message);
					break;
				case LogLevel.Info:
					Debug.Log(message);
					break;
				case LogLevel.Warn:
					Debug.LogWarning(message);
					break;
				case LogLevel.Error:
					Debug.LogError(message);
					break;
			}
		}

		public static void Log(object message, LogLevel level)
		{
			Log(message.ToString(), level);
		}

		public static void Log(string message)
		{
			Log(message, LogLevel.Info);
		}

		public static void Log(object message)
		{
			Log(message, LogLevel.Info);
		}

		public static void LogDebug(string message)
		{
			Log(message, LogLevel.Debug);
		}

		public static void LogDebug(object message)
		{
			Log(message, LogLevel.Debug);
		}

		public static void LogError(string message)
		{
			Log(message, LogLevel.Error);
		}

		public static void LogError(object message)
		{
			Log(message, LogLevel.Error);
		}

		public static void LogFine(string message)
		{
			Log(message, LogLevel.Fine);
		}

		public static void LogFine(object message)
		{
			Log(message, LogLevel.Fine);
		}

		public static void LogWarn(string message)
		{
			Log(message, LogLevel.Warn);
		}

		public static void LogWarn(object message)
		{
			Log(message, LogLevel.Warn);
		}
	}
}
