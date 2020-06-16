using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains many useful utility functions when dealing with coroutines
	/// </summary>
	public static class CoroutineUtilities
	{
		class StarterObject : MonoBehaviour
		{

		}

		static StarterObject starter;

		/// <summary>
		/// Will allow a coroutine to run while the predicate is true. If the predicate returns false, the coroutine will end
		/// </summary>
		/// <typeparam name="T">The component type</typeparam>
		/// <param name="component">The component to start the coroutine under</param>
		/// <param name="routine">The function to be executed by the coroutine</param>
		/// <param name="predicate">The predicate function that either returns true or false to control whether the coroutine should be executing or not</param>
		/// <returns>The started coroutine</returns>
		public static Coroutine RunCoroutineWhile<T>(this T component, IEnumerator routine, Func<bool> predicate) where T : MonoBehaviour
		{
			return component.StartCoroutine(RunWhile(routine, predicate));
		}

		static Func<WaitForSeconds, float> GetWFSTime = FieldUtilities.CreateGetter<WaitForSeconds, float>(typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.NonPublic));


		/// <summary>
		/// Runs an IEnumerator while the predicate is true. If the predicate returns false, then the IEnumerator will end
		/// </summary>
		/// <param name="routine">The IEnumerator to execute</param>
		/// <param name="predicate">>The predicate function that either returns true or false to control whether the Routine should be executing or not</param>
		/// <returns>The IEnumerator that executes the routine</returns>
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

		/// <summary>
		/// Starts a coroutine that isn't bound to a specific object. Please make sure that the coroutine is ended via <see cref="StopCoroutine(Coroutine)"/> to ensure it will stop
		/// </summary>
		/// <param name="routine">The Routine to execute</param>
		/// <returns>The started coroutine</returns>
		public static Coroutine StartCoroutine(IEnumerator routine)
		{
			InitStarterObject();
			return starter.StartCoroutine(routine);
		}

		/// <summary>
		/// Stops a coroutine that has been started via <see cref="StartCoroutine(IEnumerator)"/>
		/// </summary>
		/// <param name="coroutine">The coroutine to stop</param>
		public static void StopCoroutine(Coroutine coroutine)
		{
			starter.StopCoroutine(coroutine);
		}

		static void InitStarterObject()
		{
			if (starter == null)
			{
				starter = new GameObject("__COROUTINE_STARTER_OBJECT__").AddComponent<StarterObject>();
				GameObject.DontDestroyOnLoad(starter.gameObject);
			}
		}

		/// <summary>
		/// Will continously run the action function for a set period of time.
		/// </summary>
		/// <param name="time">The amount of time the action function should be called for</param>
		/// <param name="action">The action function to be continously called each frame for a set period of time. This version of the allows the action to take in the amount of elapsed time as a parameter</param>
		/// <returns></returns>
		public static IEnumerator RunForPeriod(float time, Action<float> action)
		{
			yield return RunForPeriod(time, t =>
			{
				action(t);
				return true;
			});
		}

		/// <summary>
		/// Will continously run the action function for a set period of time.
		/// </summary>
		/// <param name="time">The amount of time the action function should be called for</param>
		/// <param name="action">The action function to be continously called each frame for a set period of time.</param>
		/// <returns></returns>
		public static IEnumerator RunForPeriod(float time, Action action)
		{
			yield return RunForPeriod(time, t => action());
		}

		/// <summary>
		/// Will continously run the action function for a set period of time. The loop can be stopped prematurely by returning false from the action function
		/// </summary>
		/// <param name="time">The amount of time the action function should be called for</param>
		/// <param name="action">The action function to be continously called each frame for a set period of time. If the action returns false at any time, the loop will stop prematurely</param>
		/// <returns></returns>
		public static IEnumerator RunForPeriod(float time, Func<bool> action)
		{
			yield return RunForPeriod(time, t => action());
		}

		/// <summary>
		/// /// Will continously run the action function for a set period of time. The loop can be stopped prematurely by returning false from the action function
		/// </summary>
		/// <param name="time">The amount of time the action function should be called for</param>
		/// <param name="action">The action function to be continously called each frame for a set period of time. This version of the allows the action to take in the amount of elapsed time as a parameter. If the action returns false at any time, the loop will stop prematurely</param>
		/// <returns></returns>
		public static IEnumerator RunForPeriod(float time, Func<float, bool> action)
		{
			float timer = 0;
			while (timer < time)
			{
				yield return null;
				timer += Time.deltaTime;
				if (!action(timer))
				{
					timer = time;
				}
			}
		}




	}
}
