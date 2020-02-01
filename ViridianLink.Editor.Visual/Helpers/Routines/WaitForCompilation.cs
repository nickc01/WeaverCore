using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using ViridianLink.Editor.Helpers;

namespace ViridianLink.Editor.Routines
{
	public class WaitForCompilation : IEditorWaiter
	{
		public bool Continue(float dt)
		{
			return !EditorApplication.isCompiling;
		}
	}
}
