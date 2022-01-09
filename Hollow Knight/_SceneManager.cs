using GlobalEnums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
[System.ComponentModel.Browsable(false)]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#endif
[Serializable]
[ExecuteAlways]
public class SceneManager : MonoBehaviour
{
	private void Start()
	{
		if (Application.isPlaying)
		{
			orig_Start();
		}
		else
		{
			SceneManager.SetLighting(defaultColor, defaultIntensity);
		}
	}

	public static void SetLighting(Color ambientLightColor, float ambientLightIntensity)
	{
		float num = Mathf.Lerp(1f, ambientLightIntensity, SceneManager.AmbientIntesityMix);
		RenderSettings.ambientLight = new Color(ambientLightColor.r * num, ambientLightColor.g * num, ambientLightColor.b * num, 1f);
		RenderSettings.ambientIntensity = 1f;
	}

#if UNITY_EDITOR
	public virtual void OnValidate()
	{
		if (!Application.isPlaying)
		{
			var startFunc = WeaverTypeHelpers.GetWeaverMethod("WeaverCore.Utilities.UnboundCoroutine", "Start");
			startFunc.Invoke(null, new object[] { FrameWait(gameObject) });
			SceneManager.SetLighting(defaultColor, defaultIntensity);
		}

		IEnumerator FrameWait(GameObject obj)
		{
			yield return null;
			if (obj != null)
			{
				obj.tag = "SceneManager";
			}
		}
	}
#endif

	private void Update()
	{
		if (Application.isPlaying)
		{
			orig_Update();
		}
	}

	public int GetDarknessLevel()
	{
		return darknessLevel;
	}

	public void SetWindy(bool setting)
	{
		isWindy = setting;
	}

	public float AdjustSaturation(float originalSaturation)
	{
		if (ignorePlatformSaturationModifiers)
		{
			return originalSaturation;
		}
		return SceneManager.AdjustSaturationForPlatform(originalSaturation, mapZone);
	}

	public static float AdjustSaturationForPlatform(float originalSaturation, MapZone? mapZone = null)
	{
		if (Application.platform == RuntimePlatform.Switch)
		{
			if (mapZone != null)
			{
				MapZone? mapZone2 = mapZone;
				MapZone mapZone3 = MapZone.GODS_GLORY;
				if (mapZone2.GetValueOrDefault() == mapZone3 & mapZone2 != null)
				{
					return originalSaturation + 0.1466f;
				}
			}
			return originalSaturation + 0.17f;
		}
		return originalSaturation + 0.1466f;
	}

	private void DrawBlackBorders()
	{
		List<GameObject> list = new List<GameObject>();
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(borderPrefab);
		gameObject.transform.position = new Vector3(gm.sceneWidth + 10f, gm.sceneHeight / 2f, gameObject.transform.position.z);
		gameObject.transform.localScale = new Vector2(20f, gm.sceneHeight + 40f);
		list.Add(gameObject);
		gameObject = UnityEngine.Object.Instantiate<GameObject>(borderPrefab);
		gameObject.transform.position = new Vector3(-10f, gm.sceneHeight / 2f, gameObject.transform.position.z);
		gameObject.transform.localScale = new Vector2(20f, gm.sceneHeight + 40f);
		list.Add(gameObject);
		gameObject = UnityEngine.Object.Instantiate<GameObject>(borderPrefab);
		gameObject.transform.position = new Vector3(gm.sceneWidth / 2f, gm.sceneHeight + 10f, gameObject.transform.position.z);
		gameObject.transform.localScale = new Vector2(40f + gm.sceneWidth, 20f);
		list.Add(gameObject);
		gameObject = UnityEngine.Object.Instantiate<GameObject>(borderPrefab);
		gameObject.transform.position = new Vector3(gm.sceneWidth / 2f, -10f, gameObject.transform.position.z);
		gameObject.transform.localScale = new Vector2(40f + gm.sceneWidth, 20f);
		list.Add(gameObject);
		ModHooks.OnDrawBlackBorders(list);
	}

	private void AddSceneMapped()
	{
		if (!pd.GetVariable<List<string>>("scenesVisited").Contains(gm.GetSceneNameString()) && !manualMapTrigger && mapZone != MapZone.WHITE_PALACE && mapZone != MapZone.GODS_GLORY)
		{
			pd.GetVariable<List<string>>("scenesVisited").Add(gm.GetSceneNameString());
		}
	}

	public void UpdateSceneSettings(SceneManagerSettings sms)
	{
		mapZone = sms.mapZone;
		defaultColor = new Color(sms.defaultColor.r, sms.defaultColor.g, sms.defaultColor.b, sms.defaultColor.a);
		defaultIntensity = sms.defaultIntensity;
		saturation = sms.saturation;
		redChannel = new AnimationCurve(sms.redChannel.keys.Clone() as Keyframe[]);
		greenChannel = new AnimationCurve(sms.greenChannel.keys.Clone() as Keyframe[]);
		blueChannel = new AnimationCurve(sms.blueChannel.keys.Clone() as Keyframe[]);
		heroLightColor = new Color(sms.heroLightColor.r, sms.heroLightColor.g, sms.heroLightColor.b, sms.heroLightColor.a);
	}

	private void orig_Update()
	{
		if (isGameplayScene)
		{
			if (enviroTimer < 0.25f)
			{
				enviroTimer += Time.deltaTime;
			}
			if (!setSaturation)
			{
				if (AdjustSaturation(saturation) != gc.colorCorrectionCurves.saturation)
				{
					gc.colorCorrectionCurves.saturation = AdjustSaturation(saturation);
				}
				setSaturation = true;
			}
		}
	}

	private void orig_Start()
	{
		gm = GameManager.instance;
		gc = GameCameras.instance;
		pd = PlayerData.instance;
		if (gm.IsGameplayScene())
		{
			isGameplayScene = true;
			heroCtrl = HeroController.instance;
		}
		else
		{
			isGameplayScene = false;
		}
		gc.colorCorrectionCurves.saturation = AdjustSaturation(saturation);
		gc.colorCorrectionCurves.redChannel = redChannel;
		gc.colorCorrectionCurves.greenChannel = greenChannel;
		gc.colorCorrectionCurves.blueChannel = blueChannel;
		gc.colorCorrectionCurves.UpdateParameters();
		gc.sceneColorManager.SaturationA = AdjustSaturation(saturation);
		gc.sceneColorManager.RedA = redChannel;
		gc.sceneColorManager.GreenA = greenChannel;
		gc.sceneColorManager.BlueA = blueChannel;
		SceneManager.SetLighting(defaultColor, defaultIntensity);
		gc.sceneColorManager.AmbientColorA = defaultColor;
		gc.sceneColorManager.AmbientIntensityA = defaultIntensity;
		if (isGameplayScene)
		{
			gc.sceneColorManager.HeroLightColorA = heroLightColor;
		}
		pd.SetIntSwappedArgs(environmentType, "environmentType");
		pd.SetIntSwappedArgs(environmentType, "environmentTypeDefault");
		if (GameManager.instance)
		{
			GameManager.EnterSceneEvent temp = null;
			temp = delegate ()
			{
				AddSceneMapped();
				GameManager.instance.OnFinishedEnteringScene -= temp;
			};
			GameManager.instance.OnFinishedEnteringScene += temp;
		}
		else
		{
			AddSceneMapped();
		}
		if (isGameplayScene)
		{
			DrawBlackBorders();
		}
	}

	[Space(6f)]
	[Tooltip("This denotes the type of this scene, mainly if it is a gameplay scene or not.")]
	public SceneType sceneType = SceneType.GAMEPLAY;

	[Header("Gameplay Scene Settings")]
	[Tooltip("The area of the map this scene belongs to.")]
	[Space(6f)]
	public MapZone mapZone = MapZone.NONE;

	[Tooltip("Determines if this area is currently windy.")]
	public bool isWindy = false;

	[Tooltip("Determines if this level experiences tremors.")]
	public bool isTremorZone = false;

	[Tooltip("Set environment type on scene entry. 0 = Dust, 1 = Grass, 2 = Bone, 3 = Spa, 4 = Metal, 5 = No Effect, 6 = Wet")]
	public int environmentType = 0;

	public int darknessLevel = 0;

	public bool noLantern = false;

	[Header("Camera Color Correction Curves")]
	[Range(0f, 5f)]
	public float saturation = 0.8f;

	public bool ignorePlatformSaturationModifiers = false;

	public AnimationCurve redChannel = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve greenChannel = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve blueChannel = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Header("Ambient Light")]
	[Tooltip("The default ambient light colour for this scene.")]
	[Space(6f)]
	public Color defaultColor = new Color(0.366634f, 0.5165427f, 0.7264151f, 1f);

	[Tooltip("The intensity of the ambient light in this scene.")]
	[Range(0f, 1f)]
	public float defaultIntensity = 0.7f;

	[Header("Hero Light")]
	[Tooltip("Color of the hero's light gradient (not point lights)")]
	[Space(6f)]
	public Color heroLightColor = new Color(1f,1f,1f, 0.6588235f);

	[Header("Scene Particles")]
	public bool noParticles = false;

	public MapZone overrideParticlesWith = MapZone.NONE;

	[SerializeField]
	private float musicDelayTime = 0f;

	[SerializeField]
	private float musicTransitionTime = 0f;

	public float transitionTime = 1f;

	[HideInInspector]
	public GameObject borderPrefab;

	[Header("Mapping")]
	[Space(6f)]
	public bool manualMapTrigger = false;

	[HideInInspector]
	[SerializeField]
	protected GameObject hollowShadeObject;

	[HideInInspector]
	[SerializeField]
	protected GameObject dreamgateObject;

	private GameManager gm;

	private GameCameras gc;

	private HeroController heroCtrl;

	private PlayerData pd;

	private float enviroTimer;

	private bool enviroSent;

	private bool heroInfoSent;

	private bool setSaturation;

	private bool isGameplayScene;

	public static float AmbientIntesityMix = 0.5f;

	private const float SwitchConstant = 0.17f;

	private const float SwitchConstantGG = 0.1466f;

	private const float RegularConstant = 0.1466f;

	private readonly bool gameplayScene;
}


