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
			var gameRoutine = new GameRoutine();
			IEnumerator Routine()
			{
				yield return routine;
				gameRoutine.Done = true;
			}

			gameRoutine.routine = RoutineObject.Instance.StartCoroutine(Routine());
			return gameRoutine;
		}

		public override void Stop(UnboundCoroutine routine)
		{
			var gameRoutine = routine as GameRoutine;
			if (gameRoutine != null)
			{
				gameRoutine.Done = true;
				RoutineObject.Instance.StopCoroutine(gameRoutine.routine);
			}
		}

        public override bool IsDone(UnboundCoroutine routine)
        {
			if (routine is GameRoutine gr)
			{
				return gr.IsDone;
			}
			return true;
        }

        public class GameRoutine : UnboundCoroutine
		{
			public Coroutine routine;
			public bool Done = false;
		}

	}
}
