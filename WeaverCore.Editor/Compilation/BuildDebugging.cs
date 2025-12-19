using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Simple helper for routing build logs to both the Unity console and a temp file.
	/// </summary>
	public static class BuildDebugging
	{
		static readonly object _lock = new object();
		static string _logFilePath;

		/// <summary>
		/// Begins a new logging batch, creating a fresh log file in the temp UNITY_LOGGING directory.
		/// </summary>
		public static void StartNewBatch(string batchName)
		{
			try
			{
				var baseDir = Path.Combine(Path.GetTempPath(), "UNITY_LOGGING");
				Directory.CreateDirectory(baseDir);
				var fileName = $"{batchName}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.log";
				_logFilePath = Path.Combine(baseDir, fileName);
				WriteLineInternal("INFO", $"Starting build batch '{batchName}'");
			}
			catch (Exception ex)
			{
				Debug.LogError($"BuildDebugging failed to create log file: {ex}");
				_logFilePath = null;
			}
		}

		public static void Log(string message)
		{
			Debug.Log(message);
			WriteLineInternal("INFO", message);
		}

		public static void LogWarning(string message)
		{
			Debug.LogWarning(message);
			WriteLineInternal("WARN", message);
		}

		public static void LogError(string message)
		{
			Debug.LogError(message);
			WriteLineInternal("ERROR", message);
		}

		public static void LogException(Exception ex, string context = null)
		{
			if (!string.IsNullOrEmpty(context))
			{
				Debug.LogError($"{context}: {ex}");
				WriteLineInternal("ERROR", $"{context}: {ex}");
			}
			else
			{
				Debug.LogException(ex);
				WriteLineInternal("ERROR", ex.ToString());
			}
		}

		static void WriteLineInternal(string level, string message)
		{
			if (string.IsNullOrEmpty(_logFilePath))
			{
				return;
			}

			try
			{
				lock (_lock)
				{
					File.AppendAllText(_logFilePath, $"[{DateTime.Now:O}] [{level}] {message}{Environment.NewLine}", Encoding.UTF8);
				}
			}
			catch
			{
				// Swallow logging failures to avoid impacting the build.
			}
		}
	}
}
