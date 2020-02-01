using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using ViridianLink.Helpers;

namespace ViridianLink.Editor.Helpers
{
	public static class EditorRoutine
	{
		static List<IEnumerator<IEditorWaiter>> Routines = new List<IEnumerator<IEditorWaiter>>();

		static bool started = false;
		static float previousTime;
		static float time;


		public static void Start(IEnumerator<IEditorWaiter> routine)
		{
			if (!started)
			{
				started = true;
				previousTime = (float)EditorApplication.timeSinceStartup;
				time = (float)EditorApplication.timeSinceStartup;
				EditorApplication.update += OnUpdate;
			}
			if (routine.MoveNext())
			{
				Routines.Add(routine);
			}
			
		}

		static void OnUpdate()
		{
			previousTime = time;
			time = (float)EditorApplication.timeSinceStartup;

			float dt = time - previousTime;

			for (int i = Routines.Count - 1; i >= 0; i--)
			{
				var routine = Routines[i];
				var current = routine.Current;
				if (current == null || current.Continue(dt))
				{
					if (!routine.MoveNext())
					{
						Routines.RemoveAt(i);
					}
				}
			}
		}
	}
}
