using System.Collections;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_Shaker_I : Shaker_I
	{
		Vector3 NeutralPosition;

		Vector3 RumbleAmount = default(Vector3);
		Coroutine Shaker;

		bool shaking = false;
		int currentShakePriority = 0;


		void Awake()
		{
			NeutralPosition = transform.position;
			StartCoroutine(RumbleRoutine());
		}


		IEnumerator RumbleRoutine()
		{
			while (true)
			{
				if (RumbleAmount != default(Vector3) && !shaking)
				{
					Vector3 newPos = Vector3.Scale(RumbleAmount, new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
					transform.position = NeutralPosition + newPos;
					yield return null;
				}
				else
				{
					yield return null;
				}
			}
		}

		IEnumerator ShakeRoutine(Vector3 Amount, float duration)
		{
			try
			{
				shaking = true;
				float timer = 0f;
				do
				{
					float num = Mathf.Clamp01(1f - timer / duration);
					Vector3 a = Vector3.Scale(Amount, new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
					transform.position = NeutralPosition + (a * num);
					timer += Time.deltaTime;
					yield return null;
				} while (timer < duration);

			}
			finally
			{
				shaking = false;
				currentShakePriority = 0;
			}
		}


		public override void SetRumble(Vector3 amount)
		{
			RumbleAmount = amount;
		}

		public override void SetRumble(RumbleType type)
		{
			switch (type)
			{
				case RumbleType.None:
					SetRumble(default(Vector3));
					break;
				case RumbleType.RumblingHuge:
					SetRumble(new Vector3(1f, 1f, 0f));
					break;
				case RumbleType.RumblingBig:
					SetRumble(new Vector3(0.5f, 0.5f, 0f));
					break;
				case RumbleType.RumblingSmall:
					SetRumble(new Vector3(0.08f, 0.08f, 0f));
					break;
				case RumbleType.RumblingMed:
					SetRumble(new Vector3(0.15f, 0.15f, 0f));
					break;
				case RumbleType.RumblingFocus:
					SetRumble(new Vector3(0.015f, 0.015f, 0f));
					break;
				case RumbleType.RumblingFocus2:
					SetRumble(new Vector3(0.03f, 0.03f, 0f));
					break;
				case RumbleType.RumblingFall:
					SetRumble(new Vector3(0.015f, 0.015f, 0f));
					break;
				default:
					SetRumble(default(Vector3));
					break;
			}
		}

		public override void Shake(Vector3 amount, float duration, int priority = 100)
		{
			DoShakeRoutine(amount, duration, priority);
		}

		public override void Shake(ShakeType type)
		{
			switch (type)
			{
				case ShakeType.HugeShake:
					DoShakeRoutine(new Vector3(0.65f, 0.65f, 0f), 1f, 12);
					break;
				case ShakeType.SuperDashShake:
					DoShakeRoutine(new Vector3(0.25f, 0.25f, 0f), 1f, 8);
					break;
				case ShakeType.BigShake:
					DoShakeRoutine(new Vector3(0.5f, 0.5f, 0f), 1f, 10);
					break;
				case ShakeType.EnemyKillShake:
					DoShakeRoutine(new Vector3(0.105f, 0.105f, 0f), 0.5f, 6);
					break;
				case ShakeType.TramShake:
					DoShakeRoutine(new Vector3(0.075f, 0.075f, 0f), 2.5f, 12);
					break;
				case ShakeType.AverageShake:
					DoShakeRoutine(new Vector3(0.15f, 0.15f, 0f), 1f, 7);
					break;
				case ShakeType.BlizzardShake:
					DoShakeRoutine(new Vector3(0.15f, 0.15f, 0f), 3f, 12);
					break;
				case ShakeType.SmallShake:
					DoShakeRoutine(new Vector3(0.08f, 0.08f, 0f), 0.5f, 3);
					break;
			}
		}

		public override void StopRumbling()
		{
			SetRumble(default(Vector3));
		}

		void DoShakeRoutine(Vector3 amount, float duration, int priority)
		{
			if (priority >= currentShakePriority)
			{
				if (shaking || Shaker != null)
				{
					shaking = false;
					currentShakePriority = 0;
					Shaker = null;
				}
				currentShakePriority = priority;
				Shaker = StartCoroutine(ShakeRoutine(amount, duration));
			}
		}
	}
}
