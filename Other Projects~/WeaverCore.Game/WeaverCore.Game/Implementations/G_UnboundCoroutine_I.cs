using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	class G_UnboundCoroutine_I : UnboundCoroutine_I
	{
		class RoutineObject : MonoBehaviour
		{
			static RoutineObject _instance = null;
			public static RoutineObject Instance
			{
				get
				{
					if (_instance == null)
					{
						var obj = new GameObject("__UNBOUND_COROUTINE_RUNNER_");
						GameObject.DontDestroyOnLoad(obj);
						_instance = obj.AddComponent<RoutineObject>();
					}
					return _instance;
				}
			}
		}

		public override float DT
		{
			get
			{
				return Time.deltaTime;
			}
		}

		public override UnboundCoroutine Start(IEnumerator routine)
		{
			return new GameRoutine() { routine = RoutineObject.Instance.StartCoroutine(routine) };
		}

		public override void Stop(UnboundCoroutine routine)
		{
			var gameRoutine = routine as GameRoutine;
			if (gameRoutine != null)
			{
				RoutineObject.Instance.StopCoroutine(gameRoutine.routine);
			}
		}

		public class GameRoutine : UnboundCoroutine
		{
			public Coroutine routine;
		}

	}
}
