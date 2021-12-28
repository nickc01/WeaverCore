using System;
using WeaverCore.Implementations;

namespace WeaverCore
{
	/// <summary>
	/// Used for accessing translations
	/// </summary>
    public static class WeaverLanguage
	{
		/// <summary>
		/// Gets a translated string via a key and sheetTitle
		/// </summary>
		/// <param name="key">The language key to get the translated string under</param>
		/// <param name="sheetTitle">The sheet title to get teh translated string under</param>
		/// <param name="fallback">A fallback in case the key wasn't found</param>
		/// <returns>Returns the translated string from the language key</returns>
		public static string GetString(string key, string sheetTitle, string fallback = null)
		{
			return WeaverLanguage.impl.GetString(sheetTitle, key, fallback);
		}

		/// <summary>
		/// Gets a translated string via a key
		/// </summary>
		/// <param name="key">The language key to get the translated string under</param>
		/// <param name="fallback">A fallback in case the key wasn't found</param>
		/// <returns>Returns the translated string from the language key</returns>
		public static string GetString(string key, string fallback = null)
		{
			return WeaverLanguage.impl.GetString(key, fallback);
		}

		/// <summary>
		/// Does the language key exist?
		/// </summary>
		/// <param name="key">The language key to check for</param>
		/// <param name="sheetTitle">The sheet title to get teh translated string under</param>
		/// <returns>Returns true if the language key exists</returns>
		public static bool HasString(string key, string sheetTitle)
		{
			return WeaverLanguage.impl.HasString(sheetTitle, key);
		}

		/// <summary>
		/// Does the language key exist?
		/// </summary>
		/// <param name="key">The language key to check for</param>
		/// <returns>Returns true if the language key exists</returns>
		public static bool HasString(string key)
		{
			return WeaverLanguage.impl.HasString(key);
		}

		public static string GetStringInternal(string key, string sheetTitle)
		{
			return impl.GetStringInternal(key, sheetTitle);
		}

		public static string GetStringInternal(string key)
		{
			return impl.GetStringInternal(key);
		}

		private static WeaverLanguage_I impl = ImplFinder.GetImplementation<WeaverLanguage_I>();
	}
}
