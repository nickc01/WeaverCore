using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Implementations
{
	public class E_UnboundCoroutine_I : UnboundCoroutine_I
	{
		static List<EditorCoroutine> coroutines = new List<EditorCoroutine>();

		static double previousTime = -1.0;
		static double currentTime = -1.0;

		[OnInit]
		static void Init()
		{
			EditorApplication.update += OnUpdate;
		}

		static void OnUpdate()
		{
			if (previousTime < 0f && currentTime < 0f)
			{
				previousTime = EditorApplication.timeSinceStartup;
				currentTime = EditorApplication.timeSinceStartup;
			}
			else
			{
				previousTime = currentTime;
				currentTime = EditorApplication.timeSinceStartup;
			}

			for (int i = coroutines.Count - 1; i >= 0; i--)
			{
				if (i >= coroutines.Count)
				{
					continue;
				}
				var c = coroutines[i];
				try
				{
					if (!c.MainRoutine.MoveNext())
					{
						coroutines.Remove(c);
					}
				}
				catch (Exception e)
				{
					Debug.LogError("Editor Coroutine Error: " + e);
					coroutines.Remove(c);
				}
			}
		}

		public override float DT
		{
			get
			{
				return (float)(currentTime - previousTime);
			}
		}

		public static float Static_DT
		{
			get
			{
				return (float)(currentTime - previousTime);
			}
		}

		public override UnboundCoroutine Start(IEnumerator routine)
		{
			var c = new EditorCoroutine(routine);
			coroutines.Add(c);
			return c;
		}

		public override void Stop(UnboundCoroutine routine)
		{
			coroutines.Remove((EditorCoroutine)routine);
		}

		static Func<WaitForSeconds, float> GetWFSTime = ReflectionUtilities.CreateFieldGetter<WaitForSeconds, float>(typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.NonPublic));

		static IEnumerator RoutineUser(IEnumerator routine)
		{
			while (routine.MoveNext())
			{
				var instruction = routine.Current;

				if (instruction == null)
				{
					yield return null;
				}
				else if (instruction is int)
				{
					int frames = (int)instruction;
					for (int i = 0; i < frames; i++)
					{
						yield return null;
					}
				}
				else if (instruction is WaitForSeconds)
				{
					var wfs = instruction as WaitForSeconds;
					var time = GetWFSTime(wfs);

					for (float t = 0; t < time; t += Static_DT)
					{
						yield return null;
					}
				}
				else if (instruction is AsyncOperation)
				{
					var operation = (AsyncOperation)instruction;
					while (!operation.isDone)
					{
						yield return null;
					}
				}
				else if (instruction is CustomYieldInstruction)
				{
					var yielder = (CustomYieldInstruction)instruction;
					while (yielder.keepWaiting)
					{
						yield return null;
					}
				}
				else if (instruction is WaitForEndOfFrame)
				{
					//Do nothing
				}
				else if (instruction is WaitForFixedUpdate)
				{
					yield return null;
				}
				else if (instruction is Coroutine)
				{
					throw new Exception("Coroutine yielding in the editor is not supported");
				}
				else if (instruction is IEnumerator)
				{
					var subEnumerator = RoutineUser(instruction as IEnumerator);
					while (subEnumerator.MoveNext())
					{
						yield return subEnumerator.Current;
					}
				}
				else
				{
					yield return instruction;
				}
			}
		}

		public class EditorCoroutine : UnboundCoroutine
		{
			public IEnumerator MainRoutine;
			public GUID ID;

			public EditorCoroutine(IEnumerator routine)
			{
				MainRoutine = RoutineUser(routine);
				ID = GUID.Generate();
			}
		}
	}
}
