using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WeaverCore.Editor.Compilation
{
	public interface IAsyncBuildTask<T> : IAsyncBuildTask
	{
		new T Result { get; set; }
	}

	public interface IAsyncBuildTask
	{
		bool Completed { get; }

		object Result { get; set; }
	}
}
