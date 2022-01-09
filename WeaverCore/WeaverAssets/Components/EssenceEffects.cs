using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Used to play essence particle effects when the player defeats a boss and is transported back from the dream world
    /// </summary>
    public class EssenceEffects : MonoBehaviour
    {
        private void Awake()
        {
            if (VanishBurst == null)
            {
                VanishBurst = GetChild<ParticleSystem>("Vanish Burst Pt");
                VanishGet = GetChild<ParticleSystem>("Vanish Get Pt");
                Attack = GetChild<ParticleSystem>("Attack Pt");
                WhiteFlash = GetChild<SpriteRenderer>("White Flash");
                WhiteFlashGet = GetChild<SpriteRenderer>("White Flash Get");
            }
            DisableAll();
        }

        /// <summary>
        /// Stops all the particle effects
        /// </summary>
        public void DisableAll()
        {
            base.StopAllCoroutines();
            VanishBurst.gameObject.SetActive(false);
            VanishGet.gameObject.SetActive(false);
            Attack.gameObject.SetActive(false);
            WhiteFlash.gameObject.SetActive(false);
            WhiteFlashGet.gameObject.SetActive(false);
        }

        /// <summary>
        /// Plays the esssence particle effects
        /// </summary>
        public void PlayVanishBurstEffects()
        {
            DisableAll();
            VanishBurst.gameObject.SetActive(true);
            GameObject.Find("Dream Fall Catcher").SetActive(false);
            GameObject gameObject = GameObject.Find("_GameCameras").transform.Find("HudCamera").Find("Blanker White").gameObject;
            if (gameObject == null)
            {
                throw new Exception("Error : Blanker not found");
            }
            if (PlayMakerUtilities.PlayMakerAvailable)
            {
                PlayMakerUtilities.SetFsmFloat(gameObject, "Blanker Control", "Fade Time", 0.9f);
                EventManager.SendEventToGameObject("FADE IN", gameObject);
                string @string = PlayerData.instance.GetString("dreamReturnScene");
                UnboundCoroutine.Start(EssenceEffects.ReturnToScene(@string));
                return;
            }
            throw new Exception("Playmaker not available");
        }

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
            HeroController.instance.StopAnimationControl();
            HeroController.instance.RelinquishControl();
            PlayMakerUtilities.SetFsmBool(player, "Dream Return", "Dream Returning", true);
            HeroController.instance.EnterWithoutInput(true);
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



        private T GetChild<T>(string name)
        {
            return base.transform.Find(name).GetComponent<T>();
        }

        /// <summary>
        /// Spawns the dream particle effects. Use <see cref="PlayVanishBurstEffects"/> to play them
        /// </summary>
        /// <param name="position">The position the particles will originate at</param>
        public static EssenceEffects Spawn(Vector3 position)
        {
            if (EssenceEffects.EffectPool == null)
            {
                EssenceEffects.EffectPool = ObjectPool.Create(WeaverAssets.LoadWeaverAsset<GameObject>("Essence Effects"));
            }
            return EssenceEffects.EffectPool.Instantiate<EssenceEffects>(position, Quaternion.identity);
        }

        private static ObjectPool EffectPool;

        private ParticleSystem VanishBurst;

        private ParticleSystem VanishGet;

        private ParticleSystem Attack;

        private SpriteRenderer WhiteFlash;

        private SpriteRenderer WhiteFlashGet;
    }
}
