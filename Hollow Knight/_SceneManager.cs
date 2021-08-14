using GlobalEnums;
using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/*public class SceneManager : MonoBehaviour
{
	public MapZone mapZone;
}*/

#if UNITY_EDITOR
[System.ComponentModel.Browsable(false)]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#endif
[Serializable]
public class SceneManager : MonoBehaviour
{
	private void Start()
	{
		orig_Start();
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
			gameObject.tag = "SceneManager";
		}
	}
#endif

	private void Update()
	{
		/*if (gameplayScene && !heroInfoSent && heroCtrl != null && (heroCtrl.heroLight == null || heroCtrl.heroLight.material == null))
		{
			heroCtrl.SetDarkness(darknessLevel);
			heroInfoSent = true;
		}*/
		orig_Update();
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
		//gameObject.transform.SetPosition2D(-10f, gm.sceneHeight / 2f);
		gameObject.transform.position = new Vector3(-10f, gm.sceneHeight / 2f, gameObject.transform.position.z);
		gameObject.transform.localScale = new Vector2(20f, gm.sceneHeight + 40f);
		list.Add(gameObject);
		gameObject = UnityEngine.Object.Instantiate<GameObject>(borderPrefab);
		//gameObject.transform.SetPosition2D(gm.sceneWidth / 2f, gm.sceneHeight + 10f);
		gameObject.transform.position = new Vector3(gm.sceneWidth / 2f, gm.sceneHeight + 10f, gameObject.transform.position.z);
		gameObject.transform.localScale = new Vector2(40f + gm.sceneWidth, 20f);
		list.Add(gameObject);
		gameObject = UnityEngine.Object.Instantiate<GameObject>(borderPrefab);
		gameObject.transform.position = new Vector3(gm.sceneWidth / 2f, -10f, gameObject.transform.position.z);
		//gameObject.transform.SetPosition2D(gm.sceneWidth / 2f, -10f);
		gameObject.transform.localScale = new Vector2(40f + gm.sceneWidth, 20f);
		list.Add(gameObject);
		ModHooks.OnDrawBlackBorders(list);
	}

	// Token: 0x06001167 RID: 4455 RVA: 0x000514AC File Offset: 0x0004F6AC
	private void AddSceneMapped()
	{
		if (!pd.GetVariable<List<string>>("scenesVisited").Contains(gm.GetSceneNameString()) && !manualMapTrigger && mapZone != MapZone.WHITE_PALACE && mapZone != MapZone.GODS_GLORY)
		{
			pd.GetVariable<List<string>>("scenesVisited").Add(gm.GetSceneNameString());
		}
	}

	// Token: 0x06001168 RID: 4456 RVA: 0x00051518 File Offset: 0x0004F718
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

	// Token: 0x0600116B RID: 4459 RVA: 0x00051624 File Offset: 0x0004F824
	private void orig_Update()
	{
		if (isGameplayScene)
		{
			if (enviroTimer < 0.25f)
			{
				enviroTimer += Time.deltaTime;
			}
			/*else if (!enviroSent && heroCtrl != null)
			{
				heroCtrl.checkEnvironment();
				enviroSent = true;
			}
			if (!heroInfoSent && heroCtrl != null)
			{
				heroCtrl.heroLight.material.SetColor("_Color", Color.white);
				heroCtrl.SetDarkness(darknessLevel);
				heroInfoSent = true;
			}*/
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

	// Token: 0x0600116C RID: 4460 RVA: 0x0005171C File Offset: 0x0004F91C
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
			/*if (heroCtrl != null)
			{
				heroCtrl.heroLight.color = heroLightColor;
			}*/
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
		//TODO - FIND WORKAROUND
		/*if (atmosCue != null)
		{
			gm.AudioManager.ApplyAtmosCue(atmosCue, transitionTime);
		}
		MusicCue x = musicCue;
		if (gm.playerData.GetBool("crossroadsInfected") && infectedMusicCue != null)
		{
			x = infectedMusicCue;
		}
		if (x != null)
		{
			gm.AudioManager.ApplyMusicCue(x, musicDelayTime, musicTransitionTime, false);
		}
		if (musicSnapshot != null)
		{
			gm.AudioManager.ApplyMusicSnapshot(musicSnapshot, musicDelayTime, musicTransitionTime);
		}*/
		if (enviroSnapshot != null)
		{
			enviroSnapshot.TransitionTo(transitionTime);
		}
		if (actorSnapshot != null)
		{
			actorSnapshot.TransitionTo(transitionTime);
		}
		if (shadeSnapshot != null)
		{
			shadeSnapshot.TransitionTo(transitionTime);
		}
		if (sceneType == SceneType.GAMEPLAY)
		{
			GameObject gameObject = GameObject.FindGameObjectWithTag("Vignette");
			if (gameObject)
			{
				/*PlayMakerFSM playMakerFSM = FSMUtility.LocateFSM(gameObject, "Darkness Control");
				if (playMakerFSM)
				{
					FSMUtility.SetInt(playMakerFSM, "Darkness Level", darknessLevel);
				}
				if (!noLantern)
				{
					FSMUtility.LocateFSM(gameObject, "Darkness Control").SendEvent("RESET");
				}
				else
				{
					FSMUtility.LocateFSM(gameObject, "Darkness Control").SendEvent("SCENE RESET NO LANTERN");
					if (heroCtrl != null)
					{
						heroCtrl.wieldingLantern = false;
					}
				}*/
			}
		}
		if (isGameplayScene)
		{
			DrawBlackBorders();
		}
		if (pd.GetBool("soulLimited") && isGameplayScene && pd.GetString("shadeScene") == base.gameObject.scene.name)
		{
			/*GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(hollowShadeObject, new Vector3(pd.GetFloat("shadePositionX"), pd.GetFloat("shadePositionY"), 0.006f), Quaternion.identity);
			gameObject2.transform.SetParent(base.transform, true);
			gameObject2.transform.SetParent(null);*/
		}
		if (isGameplayScene && pd.GetString("dreamGateScene") == base.gameObject.scene.name)
		{
			/*GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(dreamgateObject, new Vector3(pd.GetFloat("dreamGateX"), pd.GetFloat("dreamGateY") - 1.429361f, -0.002f), Quaternion.identity);
			gameObject3.transform.SetParent(base.transform, true);
			gameObject3.transform.SetParent(null);*/
		}
	}

	// Token: 0x04001102 RID: 4354
	[Space(6f)]
	[Tooltip("This denotes the type of this scene, mainly if it is a gameplay scene or not.")]
	public SceneType sceneType;

	// Token: 0x04001103 RID: 4355
	[Header("Gameplay Scene Settings")]
	[Tooltip("The area of the map this scene belongs to.")]
	[Space(6f)]
	public MapZone mapZone;

	// Token: 0x04001104 RID: 4356
	[Tooltip("Determines if this area is currently windy.")]
	public bool isWindy;

	// Token: 0x04001105 RID: 4357
	[Tooltip("Determines if this level experiences tremors.")]
	public bool isTremorZone;

	// Token: 0x04001106 RID: 4358
	[Tooltip("Set environment type on scene entry. 0 = Dust, 1 = Grass, 2 = Bone, 3 = Spa, 4 = Metal, 5 = No Effect, 6 = Wet")]
	public int environmentType;

	// Token: 0x04001107 RID: 4359
	public int darknessLevel;

	// Token: 0x04001108 RID: 4360
	public bool noLantern;

	// Token: 0x04001109 RID: 4361
	[Header("Camera Color Correction Curves")]
	[Range(0f, 5f)]
	public float saturation;

	// Token: 0x0400110A RID: 4362
	public bool ignorePlatformSaturationModifiers;

	// Token: 0x0400110B RID: 4363
	public AnimationCurve redChannel;

	// Token: 0x0400110C RID: 4364
	public AnimationCurve greenChannel;

	// Token: 0x0400110D RID: 4365
	public AnimationCurve blueChannel;

	// Token: 0x0400110E RID: 4366
	[Header("Ambient Light")]
	[Tooltip("The default ambient light colour for this scene.")]
	[Space(6f)]
	public Color defaultColor;

	// Token: 0x0400110F RID: 4367
	[Tooltip("The intensity of the ambient light in this scene.")]
	[Range(0f, 1f)]
	public float defaultIntensity;

	// Token: 0x04001110 RID: 4368
	[Header("Hero Light")]
	[Tooltip("Color of the hero's light gradient (not point lights)")]
	[Space(6f)]
	public Color heroLightColor;

	// Token: 0x04001111 RID: 4369
	[Header("Scene Particles")]
	public bool noParticles;

	// Token: 0x04001112 RID: 4370
	public MapZone overrideParticlesWith;

	// Token: 0x04001113 RID: 4371
	/*[Header("Audio Snapshots")]
	[Space(6f)]
	[SerializeField]
	private AtmosCue atmosCue;

	// Token: 0x04001114 RID: 4372
	[SerializeField]
	private MusicCue musicCue;

	// Token: 0x04001115 RID: 4373
	[SerializeField]
	private MusicCue infectedMusicCue;*/

	// Token: 0x04001116 RID: 4374
	[SerializeField]
	private AudioMixerSnapshot musicSnapshot;

	// Token: 0x04001117 RID: 4375
	[SerializeField]
	private float musicDelayTime;

	// Token: 0x04001118 RID: 4376
	[SerializeField]
	private float musicTransitionTime;

	// Token: 0x04001119 RID: 4377
	public AudioMixerSnapshot atmosSnapshot;

	// Token: 0x0400111A RID: 4378
	public AudioMixerSnapshot enviroSnapshot;

	// Token: 0x0400111B RID: 4379
	public AudioMixerSnapshot actorSnapshot;

	// Token: 0x0400111C RID: 4380
	public AudioMixerSnapshot shadeSnapshot;

	// Token: 0x0400111D RID: 4381
	public float transitionTime;

	// Token: 0x0400111E RID: 4382
	[Header("Scene Border")]
	[Space(6f)]
	public GameObject borderPrefab;

	// Token: 0x0400111F RID: 4383
	[Header("Mapping")]
	[Space(6f)]
	public bool manualMapTrigger;

	// Token: 0x04001120 RID: 4384
	[Header("Object Spawns")]
	[Space(6f)]
	//public GameObject hollowShadeObject;

	// Token: 0x04001121 RID: 4385
	//public GameObject dreamgateObject;

	// Token: 0x04001122 RID: 4386
	private GameManager gm;

	// Token: 0x04001123 RID: 4387
	private GameCameras gc;

	// Token: 0x04001124 RID: 4388
	private HeroController heroCtrl;

	// Token: 0x04001125 RID: 4389
	private PlayerData pd;

	// Token: 0x04001126 RID: 4390
	private float enviroTimer;

	// Token: 0x04001127 RID: 4391
	private bool enviroSent;

	// Token: 0x04001128 RID: 4392
	private bool heroInfoSent;

	// Token: 0x04001129 RID: 4393
	private bool setSaturation;

	// Token: 0x0400112A RID: 4394
	private bool isGameplayScene;

	// Token: 0x0400112B RID: 4395
	public static float AmbientIntesityMix = 0.5f;

	// Token: 0x0400112C RID: 4396
	private const float SwitchConstant = 0.17f;

	// Token: 0x0400112D RID: 4397
	private const float SwitchConstantGG = 0.1466f;

	// Token: 0x0400112E RID: 4398
	private const float RegularConstant = 0.1466f;

	// Token: 0x0400112F RID: 4399
	private readonly bool gameplayScene;
}


