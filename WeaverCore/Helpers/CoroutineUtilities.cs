using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WeaverCore.Helpers
{
	public static class CoroutineUtilities
	{
		public static Coroutine RunCoroutineWhile<T>(this T component, IEnumerator routine, Func<bool> predicate) where T : MonoBehaviour
		{
			return component.StartCoroutine(RunWhile(routine, predicate));
		}

		static Func<WaitForSeconds, float> GetWFSTime = Fields.CreateGetter<WaitForSeconds, float>(typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.NonPublic));


		public static IEnumerator RunWhile(IEnumerator routine, Func<bool> predicate)
		{
			while (routine.MoveNext())
			{
				if (!predicate())
				{
					yield break;
				}

				var instruction = routine.Current;

				if (instruction is null)
				{
					yield return null;
				}
				else if (instruction is WaitForSeconds wfs)
				{
					var time = GetWFSTime(wfs);

					for (float t = 0; t < time; t += Time.deltaTime)
					{
						if (!predicate())
						{
							yield break;
						}
						yield return null;
					}
				}
				else if (instruction is IEnumerator e)
				{
					yield return RunWhile(e,predicate);
				}
				else
				{
					yield return instruction;
				}

			}
		}
	}
}
