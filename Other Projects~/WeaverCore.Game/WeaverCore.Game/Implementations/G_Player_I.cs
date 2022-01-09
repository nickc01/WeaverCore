using HutongGames.PlayMaker;
using System;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class G_Player_I : Player_I
	{
		public override bool HasDreamNail
		{
			get
			{
				return GameManager.instance.playerData.GetBool("hasDreamNail");
			}
		}

		public override int EssenceCollected
		{
			get
			{
				return GameManager.instance.playerData.GetInt("dreamOrbs");
			}
			set
			{
				GameManager.instance.playerData.SetInt("dreamOrbs", value);
			}
		}

		public override int EssenceSpent
		{
			get
			{
				return GameManager.instance.playerData.GetInt("dreamOrbsSpent");
			}
			set
			{
				GameManager.instance.playerData.SetInt("dreamOrbsSpend", value);
			}
		}

		public override void EnterParryState()
		{
			HeroController.instance.NailParry();
		}

		static PlayMakerFSM GetRoarFSM()
		{
			var player = Player.Player1.gameObject;

			return ActionHelpers.GetGameObjectFsm(player, "Roar Lock");
		}

		static void SendPlayerRoarEvent(string eventName)
		{
			var owner = new FsmOwnerDefault();
			owner.GameObject = Player.Player1.gameObject;
			GetRoarFSM().Fsm.Event(new FsmEventTarget()
			{
				excludeSelf = false,
				target = FsmEventTarget.EventTarget.GameObject,
				gameObject = owner,
				sendToChildren = false

			}, eventName);
		}

		public override void EnterRoarLock()
		{
			SendPlayerRoarEvent("ROAR ENTER");
		}

		public override void ExitRoarLock()
		{
			SendPlayerRoarEvent("ROAR EXIT");
		}

		public override bool HasCharmEquipped(int charmNumber)
		{
			return false;
		}

		public override void Initialize()
		{
		}

		public override void Recoil(CardinalDirection direction)
		{
			switch (direction)
			{
				case CardinalDirection.Up:
					HeroController.instance.Bounce();
					break;
				case CardinalDirection.Down:
					HeroController.instance.RecoilDown();
					break;
				case CardinalDirection.Left:
					HeroController.instance.RecoilLeft();
					break;
				default:
					HeroController.instance.RecoilRight();
					break;
			}
		}

		public override void RecoverFromParry()
		{
			HeroController.instance.NailParryRecover();
		}

		public override void RefreshSoulUI()
		{
			GameCameras.instance.soulVesselFSM.SendEvent("MP RESERVE DOWN");
			GameCameras.instance.soulVesselFSM.SendEvent("MP RESERVE UP");
			GameCameras.instance.soulOrbFSM.SendEvent("MP SET");
		}

		public override void SoulGain()
		{
			HeroController.instance.SoulGain();
		}
	}
}
