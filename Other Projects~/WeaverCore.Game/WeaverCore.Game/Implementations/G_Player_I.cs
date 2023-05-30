using HutongGames.PlayMaker;
using System;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_Player_I : Player_I
	{
		static bool inCutsceneLock = false;
		static int previousDarknessLevel = 0;

		static GameObject HudCanvas = null;
		static AudioClip dreamGhostAppear = null;

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

        public override void EnterCutsceneLock(bool playSound, int darknessLevel = -1)
        {
            if (inCutsceneLock)
			{
				throw new Exception("The player is already in a cutscene lock");
			}
			inCutsceneLock = true;

			var vignette = GameObject.FindGameObjectWithTag("Vignette");

			previousDarknessLevel = PlayMakerUtilities.GetFsmInt(vignette, "Darkness Control", "Darkness Level");

			if (previousDarknessLevel != darknessLevel)
			{
                PlayMakerUtilities.SetFsmInt(vignette, "Darkness Control", "Darkness Level", darknessLevel);

                HeroController.instance.SetDarkness(darknessLevel);

                EventManager.SendEventToGameObject("SCENE RESET", vignette);
            }

			EventManager.SendEventToGameObject("FSM CANCEL", HeroController.instance.gameObject);

			if (HudCanvas == null)
			{
				HudCanvas = GameObject.Find("Hud Canvas");
            }

			EventManager.SendEventToGameObject("OUT", HudCanvas);

			HeroController.instance.RelinquishControl();
			HeroController.instance.StartAnimationControl();

			PlayerData.instance.SetBool("disablePause", true);

			HeroController.instance.GetComponent<Rigidbody2D>().velocity = default;

			HeroController.instance.AffectedByGravity(true);

			if (playSound)
			{
				if (dreamGhostAppear == null)
				{
					dreamGhostAppear = WeaverAssets.LoadWeaverAsset<AudioClip>("dream_ghost_appear");
				}

				WeaverAudio.PlayAtPoint(dreamGhostAppear, Player.Player1.transform.position);
			}
        }

        public override void ExitCutsceneLock()
        {
            if (!inCutsceneLock)
            {
                throw new Exception("The player is already out of a cutscene lock");
            }

            if (HudCanvas == null)
            {
                HudCanvas = GameObject.Find("Hud Canvas");
            }

			HeroController.instance.RegainControl();
			HeroController.instance.StartAnimationControl();

            EventManager.SendEventToGameObject("IN", HudCanvas);

            PlayerData.instance.SetBool("disablePause", false);

            var vignette = GameObject.FindGameObjectWithTag("Vignette");

            var currentDarknessLevel = PlayMakerUtilities.GetFsmInt(vignette, "Darkness Control", "Darkness Level");

            if (previousDarknessLevel != currentDarknessLevel)
            {
                PlayMakerUtilities.SetFsmInt(vignette, "Darkness Control", "Darkness Level", previousDarknessLevel);

                HeroController.instance.SetDarkness(previousDarknessLevel);

                EventManager.SendEventToGameObject("SCENE RESET", vignette);
            }
        }
    }
}
