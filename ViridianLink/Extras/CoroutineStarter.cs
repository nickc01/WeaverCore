using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ViridianLink.Extras
{
	public static class CoroutineStarter
	{
		class StarterInternal : MonoBehaviour
		{

		}

		static StarterInternal starter;

		static CoroutineStarter()
		{
			starter = new GameObject("__COROUTINESTARTER__").AddComponent<StarterInternal>();
		}

		public static Coroutine StartCoroutine(IEnumerator routine)
		{
			return starter.StartCoroutine(routine);
		}

		public static void StopCoroutine(Coroutine routine)
		{
			starter.StopCoroutine(routine);
		}
	}
}
