using System.Reflection;

namespace WeaverCore.Editor.Utilities
{
	/// <summary>
	/// Contains some debug utility functions
	/// </summary>
	public static class DebugUtilities
	{
		static MethodInfo clearMethod = null;

		/// <summary>
		/// Clears the console of any messages
		/// </summary>
		public static void ClearLog()
		{
			if (clearMethod == null)
			{
				var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
				var type = assembly.GetType("UnityEditor.LogEntries");
				clearMethod = type.GetMethod("Clear");
			}
			clearMethod.Invoke(null, null);
		}
	}
}
