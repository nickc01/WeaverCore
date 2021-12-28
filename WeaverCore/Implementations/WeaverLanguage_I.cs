using GlobalEnums;
using Language;
using System;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class WeaverLanguage_I : IImplementation
	{
		public abstract SupportedLanguages GetCurrentLanguage();

		public abstract string GetString(string sheetName, string convoName, string fallback = null);

		public abstract string GetString(string convoName, string fallback = null);

		public abstract bool HasString(string sheetName, string convoName);

		public abstract bool HasString(string convoName);

		public abstract string GetStringInternal(string convoName);

		public abstract string GetStringInternal(string convoName, string sheetName);
	}
}
