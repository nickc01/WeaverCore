using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace WeaverCore.Editor.Helpers
{
	public class EditorRoutine
	{
		static List<IEnumerator<IEditorWaiter>> Routines = new List<IEnumerator<IEditorWaiter>>();

		static bool started = false;
		static bool initializedTimers = false;
		static float previousTime = 0;
		static float time = 0;

		IEnumerator<IEditorWaiter> Routine;

		EditorRoutine(IEnumerator<IEditorWaiter> routine)
		{
			Routine = routine;
		}

		public static EditorRoutine Start(IEnumerator<IEditorWaiter> routine)
		{
			if (!started)
			{
				started = true;
				EditorApplication.update += OnUpdate;
			}
			if (routine.MoveNext())
			{
				Routines.Add(routine);
			}
			return new EditorRoutine(routine);
		}

		public static void Stop(EditorRoutine routine)
		{
			Routines.Remove(routine.Routine);
		}

		public void Stop()
		{
			Routines.Remove(Routine);
		}

		static void OnUpdate()
		{
			if (!initializedTimers)
			{
				initializedTimers = true;
				previousTime = (float)EditorApplication.timeSinceStartup;
				time = (float)EditorApplication.timeSinceStartup;
			}
			previousTime = time;
			time = (float)EditorApplication.timeSinceStartup;

			float dt = time - previousTime;

			for (int i = Routines.Count - 1; i >= 0; i--)
			{
				if (Routines.Count > 0 && i >= Routines.Count)
				{
					i = Routines.Count - 1;
				}
				var routine = Routines[i];
				try
				{
					var current = routine.Current;
					if (current == null || !current.KeepWaiting(dt))
					{
						if (!routine.MoveNext())
						{
							Routines.Remove(routine);
						}
					}
				}
				catch (Exception)
				{
					Routines.Remove(routine);
					throw;
				}
			}
		}
	}
}
