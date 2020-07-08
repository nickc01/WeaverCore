
using UnityEngine;

namespace Modding
{
	public abstract class Loggable : ILogger
	{
		protected Loggable()
		{
			ClassName = base.GetType().Name;
		}

		public void LogFine(string message)
		{

			Debug.Log(FormatLogMessage(message));
		}

		public void LogFine(object message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void LogDebug(string message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void LogDebug(object message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void Log(string message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void Log(object message)
		{
			Debug.Log(FormatLogMessage(message));
		}

		public void LogWarn(string message)
		{
			Debug.LogWarning(FormatLogMessage(message));
		}

		public void LogWarn(object message)
		{
			Debug.LogWarning(FormatLogMessage(message));
		}

		public void LogError(string message)
		{
			Debug.LogError(FormatLogMessage(message));
		}

		public void LogError(object message)
		{
			Debug.LogError(FormatLogMessage(message));
		}

		private string FormatLogMessage(string message)
		{
			return "[" + ClassName + "] - " + message;
		}

		private string FormatLogMessage(object message)
		{
			return string.Format("[{0}] - {1}", ClassName, message);
		}

		internal string ClassName;
	}
}
