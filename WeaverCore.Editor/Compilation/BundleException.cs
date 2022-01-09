using System;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// An exception thrown when something went wrong bulding an asset bundle
	/// </summary>
	[Serializable]
	public class BundleException : Exception
	{
		public BundleException() { }
		public BundleException(string message) : base(message) { }
		public BundleException(string message, Exception inner) : base(message, inner) { }
		protected BundleException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
