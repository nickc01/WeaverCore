using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// An asynchronous build task
	/// </summary>
	/// <typeparam name="T">The result type</typeparam>
	public interface IAsyncBuildTask<T> : IAsyncBuildTask
	{
		/// <summary>
		/// The result of the task
		/// </summary>
		new T Result { get; set; }
	}

	/// <summary>
	/// An asychronous build task
	/// </summary>
	public interface IAsyncBuildTask
	{
		/// <summary>
		/// Is the task done?
		/// </summary>
		bool Completed { get; }

		/// <summary>
		/// The result of the task
		/// </summary>
		object Result { get; set; }
	}
}
