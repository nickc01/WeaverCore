using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.DataTypes;
using WeaverCore.Interfaces;
namespace WeaverCore.Game.Patches
{
	static class HitTaker_Patches
	{
		private static void HitTaker_Hit(On.HitTaker.orig_Hit orig, UnityEngine.GameObject targetGameObject, HitInstance damageInstance, int recursionDepth)
		{
			HitInfo info = Misc.ConvertHitInstance(damageInstance);
			if (targetGameObject != null)
			{
                Transform transform = targetGameObject.transform;
                for (int i = 0; i < recursionDepth; i++)
                {
                    var hittables = transform.GetComponents<IHittable>();
                    if (hittables != null && hittables.Length > 0)
                    {
                        foreach (var hittable in hittables)
                        {
                            hittable.Hit(info);
                        }
                    }
                    transform = transform.parent;
                    if (transform == null)
                    {
                        break;
                    }
                }
            }
			orig(targetGameObject, damageInstance, recursionDepth);
		}

		[OnInit]
		static void Init()
		{
			On.HitTaker.Hit += HitTaker_Hit;

            /*On.HeroController.RegainControl += HeroController_RegainControl;
            On.HeroController.RelinquishControl += HeroController_RelinquishControl;
            On.HeroController.AcceptInput += HeroController_AcceptInput;
            On.HeroController.FinishedEnteringScene += HeroController_FinishedEnteringScene;
            On.GameManager.EnterHero += GameManager_EnterHero;
            On.GameManager.HazardRespawn += GameManager_HazardRespawn;
            On.KillOnContact.OnCollisionEnter2D += KillOnContact_OnCollisionEnter2D;*/


            //On.HutongGames.PlayMaker.Fsm.Event_FsmEvent += Fsm_Event_FsmEvent;
		}

        /*private static void KillOnContact_OnCollisionEnter2D(On.KillOnContact.orig_OnCollisionEnter2D orig, KillOnContact self, Collision2D collision)
        {
            WeaverLog.Log("KILL ON CONTACT");
            WeaverLog.Log("Scene = " + self.gameObject.scene.path);
            WeaverLog.Log("GM = " + self.gameObject.name);

            orig(self, collision);
        }

        private static void GameManager_HazardRespawn(On.GameManager.orig_HazardRespawn orig, GameManager self)
        {
            WeaverLog.Log("TRUE - Setting HAZARD RESPAWN");

            orig(self);
        }

        private static void GameManager_EnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
        {
			var result = (bool)self.GetType().GetField("hazardRespawningHero", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self);
            WeaverLog.Log("Hazard Respawning Hero = " + result);

            orig(self, additiveGateSearch);
        }

        private static void HeroController_FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
        {
            WeaverLog.Log("ENTERING WITHOUT INPUT = " + self.enterWithoutInput);
            WeaverLog.Log(new System.Diagnostics.StackTrace());

            orig(self, setHazardMarker, preventRunBob);
        }

        private static void HeroController_AcceptInput(On.HeroController.orig_AcceptInput orig, HeroController self)
        {
			WeaverLog.Log("ACCEPTING INPUT");
			WeaverLog.Log(new System.Diagnostics.StackTrace());
			orig(self);
        }

        private static void HeroController_RelinquishControl(On.HeroController.orig_RelinquishControl orig, HeroController self)
        {
            //WeaverLog.Log("RELINQUISHING CONTROL");
            //WeaverLog.Log(new System.Diagnostics.StackTrace());
            orig(self);
        }

        private static void Fsm_Event_FsmEvent(On.HutongGames.PlayMaker.Fsm.orig_Event_FsmEvent orig, HutongGames.PlayMaker.Fsm self, HutongGames.PlayMaker.FsmEvent fsmEvent)
        {
			if (fsmEvent != null)
			{
                //WeaverLog.Log($"EVENT SENT = {fsmEvent.Name}");
				//WeaverLog.Log($"EVENT SOURCE = {self.GameObject?.name}-{self.Name}");
            }

			orig(self,fsmEvent);
        }

        private static void HeroController_RegainControl(On.HeroController.orig_RegainControl orig, HeroController self)
        {
			//WeaverLog.Log("REGAINING CONTROL");
			//WeaverLog.Log(new System.Diagnostics.StackTrace());
			orig(self);
        }*/


    }
}
