using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	// Token: 0x020000D9 RID: 217
	public class EssenceEffects : MonoBehaviour
	{
		// Token: 0x0600048F RID: 1167 RVA: 0x0000F7F4 File Offset: 0x0000D9F4
		private void Awake()
		{
			if (this.VanishBurst == null)
			{
				this.VanishBurst = this.GetChild<ParticleSystem>("Vanish Burst Pt");
				this.VanishGet = this.GetChild<ParticleSystem>("Vanish Get Pt");
				this.Attack = this.GetChild<ParticleSystem>("Attack Pt");
				this.WhiteFlash = this.GetChild<SpriteRenderer>("White Flash");
				this.WhiteFlashGet = this.GetChild<SpriteRenderer>("White Flash Get");
			}
			this.DisableAll();
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x0000F870 File Offset: 0x0000DA70
		public void DisableAll()
		{
			base.StopAllCoroutines();
			this.VanishBurst.gameObject.SetActive(false);
			this.VanishGet.gameObject.SetActive(false);
			this.Attack.gameObject.SetActive(false);
			this.WhiteFlash.gameObject.SetActive(false);
			this.WhiteFlashGet.gameObject.SetActive(false);
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x0000F8D8 File Offset: 0x0000DAD8
		public void PlayVanishBurstEffects()
		{
			this.DisableAll();
			this.VanishBurst.gameObject.SetActive(true);
			GameObject.Find("Dream Fall Catcher").SetActive(false);
			GameObject gameObject = GameObject.Find("_GameCameras").transform.Find("HudCamera").Find("Blanker White").gameObject;
			//WeaverLog.Log("Blanker = " + gameObject);
			if (gameObject == null)
			{
				throw new Exception("Error : Blanker not found");
			}
			//WeaverLog.Log("Blanker Enabled = " + gameObject.activeInHierarchy);
			//WeaverLog.Log("Blanker Enabled Self = " + gameObject.activeSelf);
			if (PlayMakerUtilities.PlayMakerAvailable)
			{
				PlayMakerUtilities.SetFsmFloat(gameObject, "Blanker Control", "Fade Time", 0.9f);
				//WeaverEvents.SendEventToObject(gameObject, "FADE IN");
				EventManager.SendEventToGameObject("FADE IN", gameObject);
				string @string = PlayerData.instance.GetString("dreamReturnScene");
				//WeaverLog.Log("Returning To Scene = " + @string);
				UnboundCoroutine.Start(EssenceEffects.ReturnToScene(@string));
				return;
			}
			throw new Exception("Playmaker not available");
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x0000F9F8 File Offset: 0x0000DBF8
		private static IEnumerator ReturnToScene(string scene)
		{
			yield return new WaitForSeconds(1f);
			GameObject camera = WeaverCamera.Instance.gameObject;
			if (camera == null)
			{
				throw new Exception("Camera not found");
			}
			PlayMakerUtilities.SetFsmBool(camera, "CameraFade", "No Fade", true);
			GameObject player = Player.Player1.gameObject;
			player.SendMessage("StopAnimationControl");
			player.SendMessage("RelinquishControl");
			PlayMakerUtilities.SetFsmBool(player, "Dream Return", "Dream Returning", true);
			player.SendMessage("EnterWithoutInput", true);
			GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
			{
				SceneName = scene,
				EntryGateName = "door_dreamReturn",
				EntryDelay = 0f,
				Visualization = GameManager.SceneLoadVisualizations.Dream,
				PreventCameraFadeOut = true,
				WaitForSceneTransitionCameraFade = false,
				AlwaysUnloadUnusedAssets = false
			});
			yield break;
		}



		// Token: 0x06000493 RID: 1171 RVA: 0x0000FA13 File Offset: 0x0000DC13
		private T GetChild<T>(string name)
		{
			return base.transform.Find(name).GetComponent<T>();
		}

		// Token: 0x06000494 RID: 1172 RVA: 0x0000FA26 File Offset: 0x0000DC26
		public static EssenceEffects Spawn(Vector3 position)
		{
			if (EssenceEffects.EffectPool == null)
			{
				EssenceEffects.EffectPool = ObjectPool.Create(WeaverAssets.LoadWeaverAsset<GameObject>("Essence Effects"));
			}
			return EssenceEffects.EffectPool.Instantiate<EssenceEffects>(position, Quaternion.identity);
		}

		// Token: 0x040002E4 RID: 740
		private static ObjectPool EffectPool;

		// Token: 0x040002E5 RID: 741
		private ParticleSystem VanishBurst;

		// Token: 0x040002E6 RID: 742
		private ParticleSystem VanishGet;

		// Token: 0x040002E7 RID: 743
		private ParticleSystem Attack;

		// Token: 0x040002E8 RID: 744
		private SpriteRenderer WhiteFlash;

		// Token: 0x040002E9 RID: 745
		private SpriteRenderer WhiteFlashGet;
	}
}
