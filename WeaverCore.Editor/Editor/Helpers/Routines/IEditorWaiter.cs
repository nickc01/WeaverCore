using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Editor.Helpers
{
	public interface IEditorWaiter
	{
		bool KeepWaiting(float dt);
	}
}
