using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Contains information about the current scene
    /// </summary>
    [ExecuteAlways]
	public class WeaverSceneManager : SceneManager
	{
		/// <summary>
		/// The scene manager for the currently loaded scene
		/// </summary>
		public static SceneManager CurrentSceneManager { get; set; }

		static GameObject shadePrefab;
		static GameObject dreamGatePrefab;

		[Header("Scene Info")]
		[Tooltip("Automatically sets the scene width and height")]
		[SerializeField]
		bool autoSetDimensions = true;
		[SerializeField]
		Rect sceneDimensions;

        [field: SerializeField]
        [field: Tooltip("The gate to return to if the player dies. This is only used if the player warped into this scene via dreamnail")]
        public string DreamReturnGateName { get; private set; } = "";

        [NonSerialized]
		bool sceneDimensionsRefreshed = false;

		[Header("Audio")]
		[Space]
		[SerializeField]
		[Tooltip("The music pack that is applied when the scene is loaded> If left empty, the music will be left unchanged")]
		private MusicPack music;

		[SerializeField]
		[Tooltip("If set to true, then a custom music snapshot will be applied. If set to false, then it will use the snapshot configured on the music pack")]
		bool customMusicSnapshot = false;

		[SerializeField]
		[Tooltip("The custom music snapshot to be applied")]
		Music.SnapshotType musicSnapshot = Music.SnapshotType.Normal;

		[SerializeField]
		[Tooltip("Whether the scene can be in an infected state")]
		bool canBeInfected = false;

		[SerializeField]
		[Tooltip("The music pack that is played when the scene is in it's infected state. Only used of \"Can Be Infected\" is enabled")]
		private MusicPack infectedMusic;

		[SerializeField]
		[Tooltip("The atmos snapshot pack to be applied. This determines what kind of atmosphere sounds are played in the scene")]
		private Atmos.SnapshotType atmosSnapshot = Atmos.SnapshotType.atNone;

		[SerializeField]
		[Tooltip("The actor snapshot applied. This determines whether the player sounds are enabled or disabled when entering the scene.")]
		private ActorSounds.SnapshotType actorSnapshot = ActorSounds.SnapshotType.On;

		[SerializeField]
		[Tooltip("The shade snapshot applied. This determines whether the shade music is enabled or disabled when entering the scene.")]
		private ShadeSounds.SnapshotType shadeSnapshot = ShadeSounds.SnapshotType.Away;

		[SerializeField]
		[Tooltip("The enviroment effects snapshot applied. This determines how the scene sound effects will be played when entering the scene.")]
		private EnviroEffects.SnapshotType enviroEffectsSnapshot = EnviroEffects.SnapshotType.enCliffs;


		/// <summary>
		/// The dimensions of the scene
		/// </summary>
		public Rect SceneDimensions
		{
			get
			{
				if (!sceneDimensionsRefreshed)
				{
					RefreshSceneDimensions();
				}
				return sceneDimensions;
			}
		}

		void RefreshSceneDimensions()
		{
            if (!gameObject.scene.isLoaded)
            {
				return;
            }
			sceneDimensionsRefreshed = true;
			if (autoSetDimensions)
			{
				Vector3 sceneMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity);
				Vector3 sceneMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity);
				foreach (var rootObj in gameObject.scene.GetRootGameObjects())
				{
					foreach (var collider in rootObj.GetComponentsInChildren<Collider2D>())
					{
                        if (collider.enabled && collider.gameObject.activeInHierarchy)
                        {
							var bounds = collider.bounds;
							if (bounds.min.x < sceneMin.x)
							{
								sceneMin.x = bounds.min.x;
							}
							if (bounds.min.y < sceneMin.y)
							{
								sceneMin.y = bounds.min.y;
							}

							if (bounds.max.x > sceneMax.x)
							{
								sceneMax.x = bounds.max.x;
							}
							if (bounds.max.y > sceneMax.y)
							{
								sceneMax.y = bounds.max.y;
							}
						}
					}

				}

				if (float.IsInfinity(sceneMin.x))
				{
					sceneDimensions = default;
				}
				else
				{
					sceneDimensions = new Rect(sceneMin, sceneMax - sceneMin);
				}
			}
		}

		[OnInit]
		static void Init()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
			var records = Registry.GetAllFeatures<SceneRecord>();
			var sceneRedirectEnum = Enumerable.Empty<SceneRecord.GateRedirect>();
            foreach (var record in records)
            {
				sceneRedirectEnum = sceneRedirectEnum.Concat(record.TransitionRedirects);
            }

			var redirects = sceneRedirectEnum.ToArray();

			var sceneObjects = arg0.GetRootGameObjects();
            for (int i = sceneObjects.GetLength(0) - 1; i >= 0; i--)
            {
                if (sceneObjects[i].TryGetComponent<TransitionPoint>(out var transition))
                {
                    foreach (var redirect in redirects)
					{
                        if (redirect.GateScene == arg0.name && transition.name == redirect.GateToChange)
                        {
							transition.targetScene = redirect.NewScene;
							transition.entryPoint = redirect.NewGate;
                        }
					}
				}
            }
        }

        [OnHarmonyPatch]
		static void Patch(HarmonyPatcher patcher)
		{
			var orig = typeof(SceneManager).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
			var pre = typeof(WeaverSceneManager).GetMethod("Pre_Weaver_Start_Patch", BindingFlags.NonPublic | BindingFlags.Static);
			var post = typeof(WeaverSceneManager).GetMethod("Weaver_Start_Patch", BindingFlags.NonPublic | BindingFlags.Static);
			patcher.Patch(orig, pre, post);
		}

		static Func<SceneManager, float> GetMusicDelayTime = ReflectionUtilities.CreateFieldGetter<SceneManager, float>("musicDelayTime");
		static Func<SceneManager, float> GetMusicTransitionTime = ReflectionUtilities.CreateFieldGetter<SceneManager, float>("musicTransitionTime");
		static void Weaver_Start_Patch(SceneManager __instance)
		{
			if (__instance is WeaverSceneManager wsm)
			{
				wsm.Weaver_Start();
			}
		}

		static bool Pre_Weaver_Start_Patch(SceneManager __instance)
		{
			var type = typeof(SceneManager);
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu_Title")
			{
				shadePrefab = (GameObject)type.GetField("hollowShadeObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
				dreamGatePrefab = (GameObject)type.GetField("dreamgateObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
			}
			if (__instance is WeaverSceneManager wsm)
			{
				wsm.hollowShadeObject = shadePrefab;
				wsm.dreamgateObject = dreamGatePrefab;
			}
			return true;
			
		}

		void Weaver_Start()
		{
			if (Application.isPlaying)
			{
				Atmos.ApplyAtmosSnapshot(atmosSnapshot, transitionTime);
				ActorSounds.ApplyActorSounds(actorSnapshot, transitionTime);
				ShadeSounds.ApplyShadeSounds(shadeSnapshot, transitionTime);
				EnviroEffects.ApplyEnviroEffectsSnapshot(enviroEffectsSnapshot, transitionTime);
				MusicPack x = music;
				if (PlayerData.instance.GetBool("crossroadsInfected") && infectedMusic != null)
				{
					x = infectedMusic;
				}
				var musicDelayTime = GetMusicDelayTime(this);
				var musicTransitionTime = GetMusicTransitionTime(this);
				if (x != null)
				{
					Music.PlayMusicPack(x, musicDelayTime, musicTransitionTime, !customMusicSnapshot);
				}
				if (customMusicSnapshot)
				{
					Music.ApplyMusicSnapshot(musicSnapshot, musicDelayTime, musicTransitionTime);
				}
			}
		}

		private void Awake()
		{
			if (Application.isPlaying)
			{
				CurrentSceneManager = this;
				RefreshSceneDimensions();
				if (borderPrefab == null)
				{
					borderPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Scene Border");
				}
			}
		}

        private void OnDrawGizmosSelected()
        {
            if (!autoSetDimensions)
            {
				var center = SceneDimensions.center;
				var size = SceneDimensions.size;
				Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.5f);
				Gizmos.DrawCube(new Vector3(center.x, center.y), new Vector3(size.x,size.y));
				Gizmos.color = new Color(1f, 0.5f, 0.5f, 0.5f);
				Gizmos.DrawWireCube(new Vector3(center.x, center.y), new Vector3(size.x, size.y));
            }
        }

#if UNITY_EDITOR
        public override void OnValidate()
		{
			RefreshSceneDimensions();
			base.OnValidate();
			if (!canBeInfected)
			{
				infectedMusic = null;
			}
			if (borderPrefab == null)
			{
				borderPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Scene Border");
			}
		}
#endif
	}
}
