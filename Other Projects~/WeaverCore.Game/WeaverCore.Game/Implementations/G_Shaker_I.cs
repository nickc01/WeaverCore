using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Linq;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_Shaker_I : Shaker_I
	{
		PlayMakerFSM shakerFSM;
		FsmState ShakingHugeEvent;

		bool CustomShake = false;
		float ShakeDuration = 1;
		Vector3 ShakeExtents = default;

		bool CustomRumble = false;
		Vector3 RumbleExtents = default;

		Func<ShakePosition, float> GetTimer;
		Func<ShakePosition, Vector3> GetStartingWorldPosition;
		Action<ShakePosition, float> SetTimer;

		Action<ShakePosition> StopAndReset;

		void Awake()
		{
			On.HutongGames.PlayMaker.Actions.ShakePosition.UpdateShaking += ShakePosition_UpdateShaking;
			On.HutongGames.PlayMaker.Actions.ShakePosition.OnExit += ShakePosition_OnExit;
			On.HutongGames.PlayMaker.Actions.ShakePosition.Reset += ShakePosition_Reset;
			shakerFSM = gameObject.LocateMyFSM("CameraShake");
			ShakingHugeEvent = shakerFSM.FsmStates.First(e => e.Name == "ShakingHuge");
			GetTimer = ReflectionUtilities.CreateFieldGetter<ShakePosition, float>("timer");
			GetStartingWorldPosition = ReflectionUtilities.CreateFieldGetter<ShakePosition, Vector3>("startingWorldPosition");
			SetTimer = ReflectionUtilities.CreateFieldSetter<ShakePosition, float>("timer");
			StopAndReset = ReflectionUtilities.MethodToDelegate<Action<ShakePosition>, ShakePosition>("StopAndReset");
		}

		private void ShakePosition_Reset(On.HutongGames.PlayMaker.Actions.ShakePosition.orig_Reset orig, ShakePosition self)
		{
			CustomShake = false;
			orig(self);
		}

		private void ShakePosition_OnExit(On.HutongGames.PlayMaker.Actions.ShakePosition.orig_OnExit orig, ShakePosition self)
		{
			CustomShake = false;
			orig(self);
		}

		private void ShakePosition_UpdateShaking(On.HutongGames.PlayMaker.Actions.ShakePosition.orig_UpdateShaking orig, ShakePosition self)
		{
			try
			{
				if (CustomShake && self.State.Name.Contains("Shake") && self.State.Fsm == shakerFSM.Fsm)
				{
					bool value = self.isLooping.Value;
					float num = Mathf.Clamp01(1f - GetTimer(self) / ShakeDuration);
					Vector3 a = Vector3.Scale(ShakeExtents, new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
					transform.position = GetStartingWorldPosition(self) + a * ((!value) ? num : 1f);
					//this.timer += Time.deltaTime;
					SetTimer(self, GetTimer(self) + Time.deltaTime);
					if (!value && GetTimer(self) > ShakeDuration)
					{
						CustomShake = false;
						StopAndReset(self);
						self.Fsm.Event(self.stopEvent);
						self.Finish();
					}
				}
				else if (CustomRumble && self.State.Name.Contains("Rumbling") && self.State.Fsm == shakerFSM.Fsm)
				{
					bool value = self.isLooping.Value;
					float num = Mathf.Clamp01(1f - GetTimer(self));
					Vector3 a = Vector3.Scale(RumbleExtents, new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
					transform.position = GetStartingWorldPosition(self) + a * ((!value) ? num : 1f);
					SetTimer(self, GetTimer(self) + Time.deltaTime);
					if (!value && GetTimer(self) > 1f)
					{
						CustomRumble = false;
						StopAndReset(self);
						self.Fsm.Event(self.stopEvent);
						self.Finish();
					}
				}
				else
				{
					orig(self);
				}
			}
			catch (Exception e)
			{
				WeaverLog.LogError("Camera Shaker Exception = " + e);
			}
		}

		public override void Shake(Vector3 amount, float duration, int priority = 100)
		{
			if (priority >= shakerFSM.FsmVariables.GetFsmFloat("Priority").Value)
			{
				shakerFSM.SendEvent("CANCEL SHAKE");
				ShakeDuration = duration;
				ShakeExtents = amount;
				CustomShake = true;
				shakerFSM.SendEvent("HugeShake");
			}
		}

		public override void Shake(ShakeType type)
		{
			CustomShake = false;
			switch (type)
			{
				case ShakeType.HugeShake:
					shakerFSM.SendEvent("HugeShake");
					break;
				case ShakeType.SuperDashShake:
					shakerFSM.SendEvent("SuperDashShake");
					break;
				case ShakeType.BigShake:
					shakerFSM.SendEvent("BigShake");
					break;
				case ShakeType.EnemyKillShake:
					shakerFSM.SendEvent("EnemyKillShake");
					break;
				case ShakeType.TramShake:
					shakerFSM.SendEvent("TramShake");
					break;
				case ShakeType.AverageShake:
					shakerFSM.SendEvent("AverageShake");
					break;
				case ShakeType.BlizzardShake:
					shakerFSM.SendEvent("BlizzardShake");
					break;
				case ShakeType.SmallShake:
					shakerFSM.SendEvent("SmallShake");
					break;
			}
		}

		public override void SetRumble(Vector3 amount)
		{
			StopRumbling();
			CustomRumble = true;
			RumbleExtents = amount;

			shakerFSM.FsmVariables.GetFsmBool("RumblingHuge").Value = true;
		}

		public override void SetRumble(RumbleType type)
		{
			StopRumbling();
			CustomRumble = false;
			switch (type)
			{
				case RumbleType.RumblingHuge:
					shakerFSM.FsmVariables.GetFsmBool("RumblingHuge").Value = true;
					break;
				case RumbleType.RumblingBig:
					shakerFSM.FsmVariables.GetFsmBool("RumblingBig").Value = true;
					break;
				case RumbleType.RumblingSmall:
					shakerFSM.FsmVariables.GetFsmBool("RumblingSmall").Value = true;
					break;
				case RumbleType.RumblingMed:
					shakerFSM.FsmVariables.GetFsmBool("RumblingMed").Value = true;
					break;
				case RumbleType.RumblingFocus:
					shakerFSM.FsmVariables.GetFsmBool("RumblingFocus").Value = true;
					break;
				case RumbleType.RumblingFocus2:
					shakerFSM.FsmVariables.GetFsmBool("RumblingFocus2").Value = true;
					break;
				case RumbleType.RumblingFall:
					shakerFSM.FsmVariables.GetFsmBool("RumblingFall").Value = true;
					break;
			}

		}

		public override void StopRumbling()
		{
			shakerFSM.FsmVariables.GetFsmBool("RumblingBig").Value = false;
			shakerFSM.FsmVariables.GetFsmBool("RumblingFall").Value = false;
			shakerFSM.FsmVariables.GetFsmBool("RumblingFocus").Value = false;
			shakerFSM.FsmVariables.GetFsmBool("RumblingFocus2").Value = false;
			shakerFSM.FsmVariables.GetFsmBool("RumblingHuge").Value = false;
			shakerFSM.FsmVariables.GetFsmBool("RumblingMed").Value = false;
			shakerFSM.FsmVariables.GetFsmBool("RumblingSmall").Value = false;
		}
	}
}
