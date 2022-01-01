using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using GlobalEnums;
using Modding;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Token: 0x020002D4 RID: 724
public class GameManager : MonoBehaviour
{

	internal Rect SceneDimensions { get; private set; }
	// Token: 0x170001B2 RID: 434
	// (get) Token: 0x06000F29 RID: 3881 RVA: 0x0004A69F File Offset: 0x0004889F
	public bool TimeSlowed
	{
		get
		{
			return this.timeSlowedCount > 0;
		}
	}

	// Token: 0x170001B5 RID: 437
	// (get) Token: 0x06000F2E RID: 3886 RVA: 0x0004A6CC File Offset: 0x000488CC
	/*public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}*/

	// Token: 0x170001B6 RID: 438
	// (get) Token: 0x06000F2F RID: 3887 RVA: 0x0004A6D4 File Offset: 0x000488D4
	// (set) Token: 0x06000F30 RID: 3888 RVA: 0x0004A6DC File Offset: 0x000488DC
	public CameraController cameraCtrl { get; private set; }

	// Token: 0x170001B7 RID: 439
	// (get) Token: 0x06000F31 RID: 3889 RVA: 0x0004A6E5 File Offset: 0x000488E5
	// (set) Token: 0x06000F32 RID: 3890 RVA: 0x0004A6ED File Offset: 0x000488ED
	public HeroController hero_ctrl { get; private set; }

	// Token: 0x170001B8 RID: 440
	// (get) Token: 0x06000F33 RID: 3891 RVA: 0x0004A6F6 File Offset: 0x000488F6
	// (set) Token: 0x06000F34 RID: 3892 RVA: 0x0004A6FE File Offset: 0x000488FE
	//public SpriteRenderer heroLight { get; private set; }

	// Token: 0x170001B9 RID: 441
	// (get) Token: 0x06000F35 RID: 3893 RVA: 0x0004A707 File Offset: 0x00048907
	// (set) Token: 0x06000F36 RID: 3894 RVA: 0x0004A70F File Offset: 0x0004890F
	public global::SceneManager sm { get; private set; }

	// Token: 0x170001BA RID: 442
	// (get) Token: 0x06000F37 RID: 3895 RVA: 0x0004A718 File Offset: 0x00048918
	// (set) Token: 0x06000F38 RID: 3896 RVA: 0x0004A73E File Offset: 0x0004893E
	/*public UIManager ui
	{
		[CompilerGenerated]
		get
		{
			if (this._uiInstance == null)
			{
				this._uiInstance = (UIManager)UIManager.instance;
			}
			return this._uiInstance;
		}
		[CompilerGenerated]
		private set
		{
			this._uiInstance = value;
		}
	}*/

	// Token: 0x170001BB RID: 443
	// (get) Token: 0x06000F39 RID: 3897 RVA: 0x0004A747 File Offset: 0x00048947
	// (set) Token: 0x06000F3A RID: 3898 RVA: 0x0004A74F File Offset: 0x0004894F
	//public tk2dTileMap tilemap { get; private set; }

	// Token: 0x170001BC RID: 444
	// (get) Token: 0x06000F3B RID: 3899 RVA: 0x0004A758 File Offset: 0x00048958
	// (set) Token: 0x06000F3C RID: 3900 RVA: 0x0004A760 File Offset: 0x00048960
	//public PlayMakerFSM soulOrb_fsm { get; private set; }

	// Token: 0x170001BD RID: 445
	// (get) Token: 0x06000F3D RID: 3901 RVA: 0x0004A769 File Offset: 0x00048969
	// (set) Token: 0x06000F3E RID: 3902 RVA: 0x0004A771 File Offset: 0x00048971
	//public PlayMakerFSM soulVessel_fsm { get; private set; }

	// Token: 0x170001BE RID: 446
	// (get) Token: 0x06000F3F RID: 3903 RVA: 0x0004A77A File Offset: 0x0004897A
	public float PlayTime
	{
		get
		{
			return this.sessionStartTime + this.sessionPlayTimer;
		}
	}

	// Token: 0x170001BF RID: 447
	// (get) Token: 0x06000F40 RID: 3904 RVA: 0x0004A789 File Offset: 0x00048989
	// (set) Token: 0x06000F41 RID: 3905 RVA: 0x0004A791 File Offset: 0x00048991
	public bool RespawningHero { get; set; }

	// Token: 0x14000021 RID: 33
	// (add) Token: 0x06000F42 RID: 3906 RVA: 0x0004A79C File Offset: 0x0004899C
	// (remove) Token: 0x06000F43 RID: 3907 RVA: 0x0004A7D4 File Offset: 0x000489D4
	public event GameManager.SavePersistentState SavePersistentObjects;

	// Token: 0x14000022 RID: 34
	// (add) Token: 0x06000F44 RID: 3908 RVA: 0x0004A80C File Offset: 0x00048A0C
	// (remove) Token: 0x06000F45 RID: 3909 RVA: 0x0004A844 File Offset: 0x00048A44
	public event GameManager.ResetSemiPersistentState ResetSemiPersistentObjects;

	// Token: 0x14000023 RID: 35
	// (add) Token: 0x06000F46 RID: 3910 RVA: 0x0004A87C File Offset: 0x00048A7C
	// (remove) Token: 0x06000F47 RID: 3911 RVA: 0x0004A8B4 File Offset: 0x00048AB4
	public event GameManager.DestroyPooledObjects DestroyPersonalPools;

	// Token: 0x14000024 RID: 36
	// (add) Token: 0x06000F48 RID: 3912 RVA: 0x0004A8EC File Offset: 0x00048AEC
	// (remove) Token: 0x06000F49 RID: 3913 RVA: 0x0004A924 File Offset: 0x00048B24
	public event GameManager.UnloadLevel UnloadingLevel;

	// Token: 0x14000025 RID: 37
	// (add) Token: 0x06000F4A RID: 3914 RVA: 0x0004A95C File Offset: 0x00048B5C
	// (remove) Token: 0x06000F4B RID: 3915 RVA: 0x0004A994 File Offset: 0x00048B94
	public event GameManager.RefreshLanguage RefreshLanguageText;

	// Token: 0x14000026 RID: 38
	// (add) Token: 0x06000F4C RID: 3916 RVA: 0x0004A9CC File Offset: 0x00048BCC
	// (remove) Token: 0x06000F4D RID: 3917 RVA: 0x0004AA04 File Offset: 0x00048C04
	public event GameManager.RefreshParticles RefreshParticleLevel;

	// Token: 0x14000027 RID: 39
	// (add) Token: 0x06000F4E RID: 3918 RVA: 0x0004AA3C File Offset: 0x00048C3C
	// (remove) Token: 0x06000F4F RID: 3919 RVA: 0x0004AA74 File Offset: 0x00048C74
	public event GameManager.BossLoad OnLoadedBoss;

	// Token: 0x14000028 RID: 40
	// (add) Token: 0x06000F50 RID: 3920 RVA: 0x0004AAAC File Offset: 0x00048CAC
	// (remove) Token: 0x06000F51 RID: 3921 RVA: 0x0004AAE4 File Offset: 0x00048CE4
	public event GameManager.EnterSceneEvent OnFinishedEnteringScene;

	// Token: 0x14000029 RID: 41
	// (add) Token: 0x06000F52 RID: 3922 RVA: 0x0004AB1C File Offset: 0x00048D1C
	// (remove) Token: 0x06000F53 RID: 3923 RVA: 0x0004AB54 File Offset: 0x00048D54
	public event GameManager.SceneTransitionFinishEvent OnFinishedSceneTransition;

	// Token: 0x170001C0 RID: 448
	// (get) Token: 0x06000F54 RID: 3924 RVA: 0x0004AB89 File Offset: 0x00048D89
	// (set) Token: 0x06000F55 RID: 3925 RVA: 0x0004AB91 File Offset: 0x00048D91
	public bool IsInSceneTransition { get; private set; }

	// Token: 0x170001C1 RID: 449
	// (get) Token: 0x06000F56 RID: 3926 RVA: 0x0004AB9A File Offset: 0x00048D9A
	public bool HasFinishedEnteringScene
	{
		get
		{
			return this.hasFinishedEnteringScene;
		}
	}

	// Token: 0x170001C2 RID: 450
	// (get) Token: 0x06000F57 RID: 3927 RVA: 0x0004ABA2 File Offset: 0x00048DA2
	/*public WorldInfo WorldInfo
	{
		get
		{
			return this.worldInfo;
		}
	}*/

	// Token: 0x170001C3 RID: 451
	// (get) Token: 0x06000F58 RID: 3928 RVA: 0x0004ABAA File Offset: 0x00048DAA
	public bool IsLoadingSceneTransition
	{
		get
		{
			return this.isLoading;
		}
	}

	// Token: 0x170001C4 RID: 452
	// (get) Token: 0x06000F59 RID: 3929 RVA: 0x0004ABB2 File Offset: 0x00048DB2
	public GameManager.SceneLoadVisualizations LoadVisualization
	{
		get
		{
			return this.loadVisualization;
		}
	}

	// Token: 0x170001C5 RID: 453
	// (get) Token: 0x06000F5A RID: 3930 RVA: 0x0004ABBA File Offset: 0x00048DBA
	public float CurrentLoadDuration
	{
		get
		{
			if (!this.isLoading)
			{
				return 0f;
			}
			return this.currentLoadDuration;
		}
	}

	// Token: 0x170001C6 RID: 454
	// (get) Token: 0x06000F5B RID: 3931 RVA: 0x0004ABD0 File Offset: 0x00048DD0
	public bool IsUsingCustomLoadAnimation
	{
		get
		{
			return this.isUsingCustomLoadAnimation;
		}
	}

	// Token: 0x170001C7 RID: 455
	// (get) Token: 0x06000F5C RID: 3932 RVA: 0x0004ABD8 File Offset: 0x00048DD8
	public static GameManager instance
	{
		get
		{
			if (!atteptedEditorLoad)
			{
				LoadGameManagerEditor();
			}
			if (GameManager._instance == null)
			{
				GameManager._instance = UnityEngine.Object.FindObjectOfType<GameManager>();
				if (GameManager._instance == null)
				{
					if (Application.isPlaying)
					{
						//Debug.LogError("Couldn't find a Game Manager, make sure one exists in the scene.");
					}
				}
				else if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(GameManager._instance.gameObject);
				}
			}
			return GameManager._instance;
		}
	}

	// Token: 0x170001C8 RID: 456
	// (get) Token: 0x06000F5D RID: 3933 RVA: 0x0004AC30 File Offset: 0x00048E30
	public static GameManager UnsafeInstance
	{
		get
		{
			if (!atteptedEditorLoad)
			{
				LoadGameManagerEditor();
			}
			return GameManager._instance;
		}
	}

	static bool atteptedEditorLoad = false;
	static void LoadGameManagerEditor()
	{
		atteptedEditorLoad = true;
#if UNITY_EDITOR
		var guids = UnityEditor.AssetDatabase.FindAssets("GameManager");
		foreach (var id in guids)
		{
			var path = UnityEditor.AssetDatabase.GUIDToAssetPath(id);
			if (path.Contains("WeaverCore.Editor"))
			{
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
				if (asset != null)
				{
					_instance = GameObject.Instantiate(asset).GetComponentInChildren<GameManager>();
				}
			}
		}
#endif
	}

	// Token: 0x06000F5E RID: 3934 RVA: 0x0004AC38 File Offset: 0x00048E38
	private void Awake()
	{
		gameObject.tag = "GameManager";
		if (GameManager._instance == null)
		{
			GameManager._instance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
			this.SetupGameRefs();
			return;
		}
		if (this != GameManager._instance)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		this.SetupGameRefs();
	}

	// Token: 0x06000F5F RID: 3935 RVA: 0x0004AC84 File Offset: 0x00048E84
	private void Start()
	{
		if (this == GameManager._instance)
		{
			this.SetupStatusModifiers();
		}
	}

	// Token: 0x06000F60 RID: 3936 RVA: 0x0004AC99 File Offset: 0x00048E99
	protected void Update()
	{
		if (this.isLoading)
		{
			this.currentLoadDuration += Time.unscaledDeltaTime;
		}
		else
		{
			this.currentLoadDuration = 0f;
		}
		this.IncreaseGameTimer(ref this.sessionPlayTimer);
		this.UpdateEngagement();
	}

	// Token: 0x06000F61 RID: 3937 RVA: 0x0004ACD4 File Offset: 0x00048ED4
	private void UpdateEngagement()
	{
		if (this.gameState == GameState.MAIN_MENU)
		{
			/*if (!this.ui.didLeaveEngageMenu)
			{
				if (this.ui.menuState == MainMenuState.ENGAGE_MENU)
				{
					if (Platform.Current.EngagementState != Platform.EngagementStates.Engaged)
					{
						Platform.Current.UpdateWaitingForEngagement();
						return;
					}
					if (Platform.Current.IsPlayerPrefsLoaded)
					{
						if (Platform.Current.RequiresPreferencesSyncOnEngage)
						{
							Debug.LogFormat("Syncing preferences...", Array.Empty<object>());
							UIManager.instance.LoadStoredSettings();
							Language.LoadLanguage();
							InputHandler.Instance.SetActiveGamepadType(InputManager.ActiveDevice);
							MenuStyles.Instance.LoadStyle(false, true);
						}
						this.ui.didLeaveEngageMenu = true;
						if (Platform.Current.IsSavingAllowedByEngagement)
						{
							this.ui.UIGoToMainMenu();
							return;
						}
						this.ui.UIGoToNoSaveMenu();
						return;
					}
				}
			}
			else if (Platform.Current.EngagementState != Platform.EngagementStates.Engaged && this.inputHandler.acceptingInput && !this.ui.IsAnimatingMenus && this.ui.menuState != MainMenuState.ENGAGE_MENU)
			{
				this.ui.UIGoToEngageMenu();
				this.ui.slotOne.ClearCache();
				this.ui.slotTwo.ClearCache();
				this.ui.slotThree.ClearCache();
				this.ui.slotFour.ClearCache();
				return;
			}*/
		}
		else if ((this.gameState == GameState.PLAYING || this.gameState == GameState.PAUSED)/* && Platform.Current.EngagementState != Platform.EngagementStates.Engaged*/ && !this.isEmergencyReturningToMenu)
		{
			//Debug.LogFormat("Engagement is not set up right, returning to menu...", Array.Empty<object>());
			/*this.EmergencyReturnToMenu(delegate
			{
			});*/
		}
	}

	// Token: 0x06000F62 RID: 3938 RVA: 0x0004AE94 File Offset: 0x00049094
	private void LevelActivated(Scene sceneFrom, Scene sceneTo)
	{
		if (this == GameManager._instance)
		{
			if (!this.waitForManualLevelStart)
			{
				Debug.LogFormat(this, "Performing automatic level start.", Array.Empty<object>());
				if (this.startedOnThisScene && this.IsGameplayScene())
				{
					this.tilemapDirty = true;
				}
				this.SetupSceneRefs(true);
				this.BeginScene();
				this.OnNextLevelReady();
				return;
			}
			Debug.LogFormat(this, "Deferring level start (marked as manual).", Array.Empty<object>());
		}
	}

	// Token: 0x06000F63 RID: 3939 RVA: 0x0004AF01 File Offset: 0x00049101
	private void OnDisable()
	{
		UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= this.LevelActivated;
	}

	// Token: 0x06000F64 RID: 3940 RVA: 0x0004AF14 File Offset: 0x00049114
	private void OnApplicationQuit()
	{
		//this.orig_OnApplicationQuit();
		ModHooks.OnApplicationQuit();
	}

	// Token: 0x06000F65 RID: 3941 RVA: 0x0004AF21 File Offset: 0x00049121
	public void BeginSceneTransition(GameManager.SceneLoadInfo info)
	{
		//info.SceneName = ModHooks.BeforeSceneLoad(info.SceneName);
		this.orig_BeginSceneTransition(info);
	}

	// Token: 0x1400002A RID: 42
	// (add) Token: 0x06000F66 RID: 3942 RVA: 0x0004AF3C File Offset: 0x0004913C
	// (remove) Token: 0x06000F67 RID: 3943 RVA: 0x0004AF70 File Offset: 0x00049170
	public static event GameManager.SceneTransitionBeganDelegate SceneTransitionBegan;

	// Token: 0x06000F68 RID: 3944 RVA: 0x0004AFA3 File Offset: 0x000491A3
	private IEnumerator BeginSceneTransitionRoutine(GameManager.SceneLoadInfo info)
	{
		if (this.sceneLoad != null)
		{
			Debug.LogErrorFormat(this, "Cannot scene transition to {0}, while a scene transition is in progress", new object[]
			{
				info.SceneName
			});
			yield break;
		}
		this.IsInSceneTransition = true;
		this.sceneLoad = new SceneLoad(this, info.SceneName);
		this.isLoading = true;
		this.loadVisualization = info.Visualization;
		if (this.hero_ctrl != null)
		{
			if (this.hero_ctrl.cState.superDashing)
			{
				this.hero_ctrl.exitedSuperDashing = true;
			}
			if (this.hero_ctrl.cState.spellQuake)
			{
				this.hero_ctrl.exitedQuake = true;
			}
			//this.hero_ctrl.proxyFSM.SendEvent("HeroCtrl-LeavingScene");
			this.hero_ctrl.SetHeroParent(null);
		}
		if (!info.IsFirstLevelForPlayer)
		{
			this.NoLongerFirstGame();
		}
		this.SaveLevelState();
		this.SetState(GameState.EXITING_LEVEL);
		this.entryGateName = (info.EntryGateName ?? "");
		this.targetScene = info.SceneName;
		if (this.hero_ctrl != null)
		{
			this.hero_ctrl.LeaveScene(info.HeroLeaveDirection);
		}
		if (!info.PreventCameraFadeOut)
		{
			this.cameraCtrl.FreezeInPlace(true);
			this.cameraCtrl.FadeOut(CameraFadeType.LEVEL_TRANSITION);
		}
		this.tilemapDirty = true;
		this.startedOnThisScene = false;
		this.nextSceneName = info.SceneName;
		this.waitForManualLevelStart = true;
		if (this.UnloadingLevel != null)
		{
			this.UnloadingLevel();
		}
		string lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		this.sceneLoad.FetchComplete += delegate ()
		{
			info.NotifyFetchComplete();
		};
		this.sceneLoad.WillActivate += delegate ()
		{
			if (this.DestroyPersonalPools != null)
			{
				this.DestroyPersonalPools();
			}
			this.entryDelay = info.EntryDelay;
		};
		this.sceneLoad.ActivationComplete += delegate ()
		{
			UnityEngine.SceneManagement.SceneManager.UnloadScene(lastSceneName);
			this.RefreshTilemapInfo(info.SceneName);
			this.sceneLoad.IsUnloadAssetsRequired = (info.AlwaysUnloadUnusedAssets || this.IsUnloadAssetsRequired(lastSceneName, info.SceneName));
			bool flag2 = true;
			if (!this.sceneLoad.IsUnloadAssetsRequired)
			{
				float? beginTime = this.sceneLoad.BeginTime;
				/*if (beginTime != null && Time.realtimeSinceStartup - beginTime.Value > Platform.Current.MaximumLoadDurationForNonCriticalGarbageCollection && this.sceneLoadsWithoutGarbageCollect < Platform.Current.MaximumSceneTransitionsWithoutNonCriticalGarbageCollection)
				{
					flag2 = false;
				}*/
			}
			if (flag2)
			{
				this.sceneLoadsWithoutGarbageCollect = 0;
			}
			else
			{
				this.sceneLoadsWithoutGarbageCollect++;
			}
			this.sceneLoad.IsGarbageCollectRequired = flag2;
			//Platform.Current.FlushSocialEvents();
		};
		this.sceneLoad.Complete += delegate ()
		{
			this.SetupSceneRefs(false);
			this.BeginScene();
			/*if (this.gameMap != null)
			{
				this.gameMap.GetComponent<GameMap>().LevelReady();
			}*/
		};
		this.sceneLoad.Finish += delegate ()
		{
			this.sceneLoad = null;
			this.isLoading = false;
			this.waitForManualLevelStart = false;
			info.NotifyFinished();
			this.OnNextLevelReady();
			this.IsInSceneTransition = false;
			if (this.OnFinishedSceneTransition != null)
			{
				this.OnFinishedSceneTransition();
			}
		};
		if (GameManager.SceneTransitionBegan != null)
		{
			try
			{
				GameManager.SceneTransitionBegan(this.sceneLoad);
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception in responders to GameManager.SceneTransitionBegan. Attempting to continue load regardless.");
				Debug.LogException(exception);
			}
		}
		this.sceneLoad.IsFetchAllowed = (!info.forceWaitFetch && (/*Platform.Current.FetchScenesBeforeFade || */info.PreventCameraFadeOut));
		this.sceneLoad.IsActivationAllowed = false;
		this.sceneLoad.Begin();
		float cameraFadeTimer = 0.5f;
		for (; ; )
		{
			bool flag = false;
			cameraFadeTimer -= Time.unscaledDeltaTime;
			if (info.WaitForSceneTransitionCameraFade && cameraFadeTimer > 0f)
			{
				flag = true;
			}
			if (!info.IsReadyToActivate())
			{
				flag = true;
			}
			if (!flag)
			{
				break;
			}
			yield return null;
		}
		this.sceneLoad.IsFetchAllowed = true;
		this.sceneLoad.IsActivationAllowed = true;
		yield break;
	}

	// Token: 0x06000F69 RID: 3945 RVA: 0x0004AFB9 File Offset: 0x000491B9
	public IEnumerator TransitionScene(TransitionPoint gate)
	{
		Debug.LogError("TransitionScene(TransitionPoint) is no longer supported");
		this.callingGate = gate;
		if (this.hero_ctrl.cState.superDashing)
		{
			this.hero_ctrl.exitedSuperDashing = true;
		}
		if (this.hero_ctrl.cState.spellQuake)
		{
			this.hero_ctrl.exitedQuake = true;
		}
		//this.hero_ctrl.GetComponent<PlayMakerFSM>().SendEvent("HeroCtrl-LeavingScene");
		this.NoLongerFirstGame();
		this.SaveLevelState();
		this.SetState(GameState.EXITING_LEVEL);
		this.entryGateName = gate.entryPoint;
		this.targetScene = gate.targetScene;
		this.hero_ctrl.LeaveScene(new GatePosition?(gate.GetGatePosition()));
		this.cameraCtrl.FreezeInPlace(true);
		this.cameraCtrl.FadeOut(CameraFadeType.LEVEL_TRANSITION);
		this.hasFinishedEnteringScene = false;
		yield return new WaitForSeconds(0.5f);
		this.LeftScene(true);
		yield break;
	}

	// Token: 0x06000F6A RID: 3946 RVA: 0x0004AFD0 File Offset: 0x000491D0
	public void ChangeToScene(string targetScene, string entryGateName, float pauseBeforeEnter)
	{
		if (this.hero_ctrl != null)
		{
			//this.hero_ctrl.proxyFSM.SendEvent("HeroCtrl-LeavingScene");
			this.hero_ctrl.transform.SetParent(null);
		}
		this.NoLongerFirstGame();
		this.SaveLevelState();
		this.SetState(GameState.EXITING_LEVEL);
		this.entryGateName = entryGateName;
		this.targetScene = targetScene;
		this.entryDelay = pauseBeforeEnter;
		this.cameraCtrl.FreezeInPlace(false);
		if (this.hero_ctrl != null)
		{
			this.hero_ctrl.ResetState();
		}
		this.LeftScene(false);
	}

	// Token: 0x06000F6B RID: 3947 RVA: 0x0004B068 File Offset: 0x00049268
	public void WarpToDreamGate()
	{
		this.entryGateName = "dreamGate";
		this.targetScene = this.playerData.GetString("dreamGateScene");
		this.entryDelay = 0f;
		this.cameraCtrl.FreezeInPlace(false);
		this.BeginSceneTransition(new GameManager.SceneLoadInfo
		{
			AlwaysUnloadUnusedAssets = true,
			EntryGateName = "dreamGate",
			PreventCameraFadeOut = true,
			SceneName = this.playerData.GetString("dreamGateScene"),
			Visualization = GameManager.SceneLoadVisualizations.Dream
		});
	}

	// Token: 0x06000F6C RID: 3948 RVA: 0x0004B0EE File Offset: 0x000492EE
	public void LeftScene(bool doAdditiveLoad = false)
	{
		UnityEngine.SceneManagement.SceneManager.GetSceneByName(this.targetScene);
		if (doAdditiveLoad)
		{
			base.StartCoroutine(this.LoadSceneAdditive(this.targetScene));
			return;
		}
		this.LoadScene(this.targetScene);
	}

	// Token: 0x06000F6D RID: 3949 RVA: 0x0004B11F File Offset: 0x0004931F
	public IEnumerator PlayerDead(float waitTime)
	{
		ModHooks.OnBeforePlayerDead();
		yield return this.orig_PlayerDead(waitTime);
		ModHooks.OnAfterPlayerDead();
		yield break;
	}

	// Token: 0x06000F6E RID: 3950 RVA: 0x0004B135 File Offset: 0x00049335
	public IEnumerator PlayerDeadFromHazard(float waitTime)
	{
		this.cameraCtrl.FreezeInPlace(true);
		this.NoLongerFirstGame();
		this.SaveLevelState();
		yield return new WaitForSeconds(waitTime);
		this.cameraCtrl.FadeOut(CameraFadeType.HERO_HAZARD_DEATH);
		yield return new WaitForSeconds(0.8f);
		//PlayMakerFSM.BroadcastEvent("HAZARD RELOAD");
		this.HazardRespawn();
		yield break;
	}

	// Token: 0x06000F6F RID: 3951 RVA: 0x0004B14C File Offset: 0x0004934C
	public void ReadyForRespawn(bool isFirstLevelForPlayer)
	{
		this.RespawningHero = true;
		this.BeginSceneTransition(new GameManager.SceneLoadInfo
		{
			PreventCameraFadeOut = true,
			WaitForSceneTransitionCameraFade = false,
			EntryGateName = "",
			SceneName = this.playerData.GetString("respawnScene"),
			Visualization = (isFirstLevelForPlayer ? GameManager.SceneLoadVisualizations.ContinueFromSave : GameManager.SceneLoadVisualizations.Default),
			AlwaysUnloadUnusedAssets = true,
			IsFirstLevelForPlayer = isFirstLevelForPlayer
		});
	}

	// Token: 0x06000F70 RID: 3952 RVA: 0x0004B1B5 File Offset: 0x000493B5
	public void HazardRespawn()
	{
		this.hazardRespawningHero = true;
		this.entryGateName = "";
		this.cameraCtrl.ResetStartTimer();
		this.cameraCtrl.camTarget.mode = CameraTarget.TargetMode.FOLLOW_HERO;
		this.EnterHero(false);
	}

	// Token: 0x06000F71 RID: 3953 RVA: 0x0004B1EC File Offset: 0x000493EC
	public void TimePasses()
	{
		this.playerData.SetBool("bankerTheftCheck", true);
		if (this.playerData.GetBool("defeatedDungDefender") && !this.playerData.GetBool("dungDefenderEncounterReady"))
		{
			this.playerData.SetBoolSwappedArgs(true, "dungDefenderEncounterReady");
		}
		if (this.playerData.GetBool("nailsmithCliff") && !this.playerData.GetBool("nailsmithKilled"))
		{
			this.playerData.SetBoolSwappedArgs(true, "nailsmithSpared");
		}
		if (this.playerData.GetBool("hasDashSlash") && this.playerData.GetBool("nailsmithSpared"))
		{
			this.playerData.SetBoolSwappedArgs(true, "nailsmithSheo");
		}
		if (this.playerData.GetBool("brettaRescued") && this.sm.mapZone.ToString() != "TOWN")
		{
			if (UnityEngine.Random.Range(0f, 1f) >= 0.5f)
			{
				this.playerData.SetIntSwappedArgs(0, "brettaPosition");
			}
			else
			{
				this.playerData.SetIntSwappedArgs(1, "brettaPosition");
			}
			if (this.playerData.GetBool("brettaSeenBench") && !this.playerData.GetBool("brettaSeenBenchDiary"))
			{
				this.playerData.SetBoolSwappedArgs(true, "brettaSeenBenchDiary");
			}
			if (this.playerData.GetBool("brettaSeenBed") && !this.playerData.GetBool("brettaSeenBedDiary"))
			{
				this.playerData.SetBoolSwappedArgs(true, "brettaSeenBedDiary");
			}
		}
		if (this.playerData.GetBool("legEaterLeft") && this.playerData.GetBool("defeatedNightmareGrimm") && this.sm.mapZone.ToString() != "TOWN" && this.playerData.GetBool("divineFinalConvo"))
		{
			this.playerData.SetBoolSwappedArgs(false, "divineInTown");
		}
		if (this.playerData.GetBool("zoteSpokenCity"))
		{
			this.playerData.SetBoolSwappedArgs(true, "zoteLeftCity");
		}
		if (this.playerData.GetBool("finalGrubRewardCollected"))
		{
			this.playerData.SetBoolSwappedArgs(true, "fatGrubKing");
		}
		if (this.playerData.GetBool("dungDefenderAwoken"))
		{
			this.playerData.SetBoolSwappedArgs(true, "dungDefenderLeft");
		}
		if (this.playerData.GetBool("scaredFlukeHermitEncountered"))
		{
			this.playerData.SetBoolSwappedArgs(true, "scaredFlukeHermitReturned");
		}
		if (this.playerData.GetBool("xunFlowerGiven"))
		{
			this.playerData.SetBoolSwappedArgs(true, "extraFlowerAppear");
		}
		if (this.playerData.GetBool("nailsmithKilled") && this.playerData.GetBool("godseekerSpokenAwake"))
		{
			this.playerData.SetBoolSwappedArgs(true, "nailsmithCorpseAppeared");
		}
	}

	// Token: 0x06000F72 RID: 3954 RVA: 0x0004B4D4 File Offset: 0x000496D4
	public void FadeSceneIn()
	{
		this.cameraCtrl.FadeSceneIn();
	}

	// Token: 0x06000F73 RID: 3955 RVA: 0x0004B4E1 File Offset: 0x000496E1
	public IEnumerator FadeSceneInWithDelay(float delay)
	{
		if (delay >= 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		else
		{
			yield return null;
		}
		this.FadeSceneIn();
		yield break;
	}

	// Token: 0x06000F74 RID: 3956 RVA: 0x0004B4F7 File Offset: 0x000496F7
	public bool IsGamePaused()
	{
		return this.gameState == GameState.PAUSED;
	}

	// Token: 0x06000F75 RID: 3957 RVA: 0x0004B505 File Offset: 0x00049705
	public void SetGameMap(GameObject go_gameMap)
	{
		this.gameMap = go_gameMap;
	}

	// Token: 0x06000F76 RID: 3958 RVA: 0x0004B50E File Offset: 0x0004970E
	public void CalculateNotchesUsed()
	{
		//this.playerData.CalculateNotchesUsed();
	}

	// Token: 0x06000F77 RID: 3959 RVA: 0x0004B51B File Offset: 0x0004971B
	public string GetLanguageAsString()
	{
		return editorLanguage.ToString();
	}

	// Token: 0x06000F78 RID: 3960 RVA: 0x0004B533 File Offset: 0x00049733
	public string GetEntryGateName()
	{
		return this.entryGateName;
	}

	// Token: 0x06000F79 RID: 3961 RVA: 0x0004B53B File Offset: 0x0004973B
	public void SetPlayerDataBool(string boolName, bool value)
	{
		this.playerData.SetBool(boolName, value);
	}

	// Token: 0x06000F7A RID: 3962 RVA: 0x0004B54A File Offset: 0x0004974A
	public void SetPlayerDataInt(string intName, int value)
	{
		this.playerData.SetInt(intName, value);
	}

	// Token: 0x06000F7B RID: 3963 RVA: 0x0004B559 File Offset: 0x00049759
	public void SetPlayerDataFloat(string floatName, float value)
	{
		this.playerData.SetFloat(floatName, value);
	}

	// Token: 0x06000F7C RID: 3964 RVA: 0x0004B568 File Offset: 0x00049768
	public void SetPlayerDataString(string stringName, string value)
	{
		this.playerData.SetString(stringName, value);
	}

	// Token: 0x06000F7D RID: 3965 RVA: 0x0004B577 File Offset: 0x00049777
	public void IncrementPlayerDataInt(string intName)
	{
		this.playerData.IncrementInt(intName);
	}

	// Token: 0x06000F7E RID: 3966 RVA: 0x0004B585 File Offset: 0x00049785
	public void DecrementPlayerDataInt(string intName)
	{
		this.playerData.DecrementInt(intName);
	}

	// Token: 0x06000F7F RID: 3967 RVA: 0x0004B593 File Offset: 0x00049793
	public void IntAdd(string intName, int amount)
	{
		this.playerData.IntAdd(intName, amount);
	}

	// Token: 0x06000F80 RID: 3968 RVA: 0x0004B5A2 File Offset: 0x000497A2
	public bool GetPlayerDataBool(string boolName)
	{
		return this.playerData.GetBool(boolName);
	}

	// Token: 0x06000F81 RID: 3969 RVA: 0x0004B5B0 File Offset: 0x000497B0
	public int GetPlayerDataInt(string intName)
	{
		return this.playerData.GetInt(intName);
	}

	// Token: 0x06000F82 RID: 3970 RVA: 0x0004B5BE File Offset: 0x000497BE
	public float GetPlayerDataFloat(string floatName)
	{
		return this.playerData.GetFloat(floatName);
	}

	// Token: 0x06000F83 RID: 3971 RVA: 0x0004B5CC File Offset: 0x000497CC
	public string GetPlayerDataString(string stringName)
	{
		return this.playerData.GetString(stringName);
	}

	// Token: 0x06000F84 RID: 3972 RVA: 0x0004B5DA File Offset: 0x000497DA
	public void SetPlayerDataVector3(string vectorName, Vector3 value)
	{
		this.playerData.SetVector3(vectorName, value);
	}

	// Token: 0x06000F85 RID: 3973 RVA: 0x0004B5E9 File Offset: 0x000497E9
	public Vector3 GetPlayerDataVector3(string vectorName)
	{
		return this.playerData.GetVector3(vectorName);
	}

	// Token: 0x06000F86 RID: 3974 RVA: 0x0004B5F7 File Offset: 0x000497F7
	public T GetPlayerDataVariable<T>(string fieldName)
	{
		return this.playerData.GetVariable<T>(fieldName);
	}

	// Token: 0x06000F87 RID: 3975 RVA: 0x0004B605 File Offset: 0x00049805
	public void SetPlayerDataVariable<T>(string fieldName, T value)
	{
		this.playerData.SetVariable<T>(fieldName, value);
	}

	// Token: 0x06000F88 RID: 3976 RVA: 0x0004B614 File Offset: 0x00049814
	public void EquipCharm(int charmNum)
	{
		//this.playerData.EquipCharm(charmNum);
	}

	// Token: 0x06000F89 RID: 3977 RVA: 0x0004B622 File Offset: 0x00049822
	public void UnequipCharm(int charmNum)
	{
		//this.playerData.UnequipCharm(charmNum);
	}

	// Token: 0x06000F8A RID: 3978 RVA: 0x0004B630 File Offset: 0x00049830
	public void RefreshOvercharm()
	{
		if (this.playerData.GetInt("charmSlotsFilled") > this.playerData.GetInt("charmSlots"))
		{
			this.playerData.SetBoolSwappedArgs(true, "overcharmed");
			return;
		}
		this.playerData.SetBoolSwappedArgs(false, "overcharmed");
	}

	// Token: 0x06000F8B RID: 3979 RVA: 0x0004B682 File Offset: 0x00049882
	public void UpdateBlueHealth()
	{
		//this.playerData.UpdateBlueHealth();
	}

	// Token: 0x06000F8C RID: 3980 RVA: 0x0004B68F File Offset: 0x0004988F
	public void SetCurrentMapZoneAsRespawn()
	{
		this.playerData.SetVariableSwappedArgs<MapZone>(this.sm.mapZone, "mapZone");
	}

	// Token: 0x06000F8D RID: 3981 RVA: 0x0004B6AC File Offset: 0x000498AC
	public void SetMapZoneToSpecific(string mapZone)
	{
		object obj = Enum.Parse(typeof(MapZone), mapZone);
		if (obj != null)
		{
			this.playerData.SetVariableSwappedArgs<MapZone>((MapZone)obj, "mapZone");
			return;
		}
		Debug.LogError("Couldn't convert " + mapZone + " to a MapZone");
	}

	// Token: 0x06000F8E RID: 3982 RVA: 0x0004B6F9 File Offset: 0x000498F9
	public void StartSoulLimiter()
	{
		//this.playerData.StartSoulLimiter();
	}

	// Token: 0x06000F8F RID: 3983 RVA: 0x0004B706 File Offset: 0x00049906
	public void EndSoulLimiter()
	{
		//this.playerData.EndSoulLimiter();
	}

	// Token: 0x06000F90 RID: 3984 RVA: 0x0004B713 File Offset: 0x00049913
	public bool UpdateGameMap()
	{
		return false;
		//return this.playerData.UpdateGameMap();
	}

	// Token: 0x06000F91 RID: 3985 RVA: 0x0004B720 File Offset: 0x00049920
	public void CheckAllMaps()
	{
		//this.playerData.CheckAllMaps();
	}

	// Token: 0x06000F92 RID: 3986 RVA: 0x0004B72D File Offset: 0x0004992D
	public void AddToScenesVisited(string scene)
	{
		if (!this.playerData.GetVariable<List<string>>("scenesVisited").Contains(scene))
		{
			this.playerData.GetVariable<List<string>>("scenesVisited").Add(scene);
		}
	}

	// Token: 0x06000F93 RID: 3987 RVA: 0x0004B75D File Offset: 0x0004995D
	public bool GetIsSceneVisited(string scene)
	{
		return this.playerData.GetVariable<List<string>>("scenesVisited").Contains(scene);
	}

	// Token: 0x06000F94 RID: 3988 RVA: 0x0004B775 File Offset: 0x00049975
	public void AddToBenchList()
	{
		if (!this.playerData.GetVariable<List<string>>("scenesEncounteredBench").Contains(this.GetSceneNameString()))
		{
			this.playerData.GetVariable<List<string>>("scenesEncounteredBench").Add(this.GetSceneNameString());
		}
	}

	// Token: 0x06000F95 RID: 3989 RVA: 0x0004B7B0 File Offset: 0x000499B0
	public void AddToGrubList()
	{
		if (!this.playerData.GetVariable<List<string>>("scenesGrubRescued").Contains(this.GetSceneNameString()))
		{
			this.playerData.GetVariable<List<string>>("scenesGrubRescued").Add(this.GetSceneNameString());
			/*if (this.gameMap != null)
			{
				this.gameMap.GetComponent<GameMap>().SetupMap(true);
			}*/
		}
	}

	// Token: 0x06000F96 RID: 3990 RVA: 0x0004B814 File Offset: 0x00049A14
	public void AddToFlameList()
	{
		if (!this.playerData.GetVariable<List<string>>("scenesFlameCollected").Contains(this.GetSceneNameString()))
		{
			this.playerData.GetVariable<List<string>>("scenesFlameCollected").Add(this.GetSceneNameString());
		}
	}

	// Token: 0x06000F97 RID: 3991 RVA: 0x0004B84E File Offset: 0x00049A4E
	public void AddToCocoonList()
	{
		if (!this.playerData.GetVariable<List<string>>("scenesEncounteredCocoon").Contains(this.GetSceneNameString()))
		{
			this.playerData.GetVariable<List<string>>("scenesEncounteredCocoon").Add(this.GetSceneNameString());
		}
	}

	// Token: 0x06000F98 RID: 3992 RVA: 0x0004B888 File Offset: 0x00049A88
	public void AddToDreamPlantList()
	{
		if (!this.playerData.GetVariable<List<string>>("scenesEncounteredDreamPlant").Contains(this.GetSceneNameString()))
		{
			this.playerData.GetVariable<List<string>>("scenesEncounteredDreamPlant").Add(this.GetSceneNameString());
		}
	}

	// Token: 0x06000F99 RID: 3993 RVA: 0x0004B8C2 File Offset: 0x00049AC2
	public void AddToDreamPlantCList()
	{
		if (!this.playerData.GetVariable<List<string>>("scenesEncounteredDreamPlantC").Contains(this.GetSceneNameString()))
		{
			this.playerData.GetVariable<List<string>>("scenesEncounteredDreamPlantC").Add(this.GetSceneNameString());
		}
	}

	// Token: 0x06000F9A RID: 3994 RVA: 0x0004B8FC File Offset: 0x00049AFC
	public void CountGameCompletion()
	{
		//this.playerData.CountGameCompletion();
	}

	// Token: 0x06000F9B RID: 3995 RVA: 0x0004B909 File Offset: 0x00049B09
	public void CountCharms()
	{
		//this.playerData.CountCharms();
	}

	// Token: 0x06000F9C RID: 3996 RVA: 0x0004B916 File Offset: 0x00049B16
	public void CountJournalEntries()
	{
		//this.playerData.CountJournalEntries();
	}

	// Token: 0x06000F9D RID: 3997 RVA: 0x0004B923 File Offset: 0x00049B23
	public void ActivateTestingCheats()
	{
		//this.playerData.ActivateTestingCheats();
	}

	// Token: 0x06000F9E RID: 3998 RVA: 0x0004B930 File Offset: 0x00049B30
	public void GetAllPowerups()
	{
		//this.playerData.GetAllPowerups();
	}

	// Token: 0x06000F9F RID: 3999 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_death()
	{
	}

	// Token: 0x06000FA0 RID: 4000 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_rescueGrub()
	{
	}

	// Token: 0x06000FA1 RID: 4001 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_defeatedShade()
	{
	}

	// Token: 0x06000FA2 RID: 4002 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_discoveredArea(string areaName)
	{
	}

	// Token: 0x06000FA3 RID: 4003 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_travelledToArea(string areaName)
	{
	}

	// Token: 0x06000FA4 RID: 4004 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_bankDeposit(int amount)
	{
	}

	// Token: 0x06000FA5 RID: 4005 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_bankWithdraw(int amount)
	{
	}

	// Token: 0x06000FA6 RID: 4006 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_boughtCorniferMap(string map)
	{
	}

	// Token: 0x06000FA7 RID: 4007 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_visited(string visited)
	{
	}

	// Token: 0x06000FA8 RID: 4008 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_defeated(string defeated)
	{
	}

	// Token: 0x06000FA9 RID: 4009 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_jiji()
	{
	}

	// Token: 0x06000FAA RID: 4010 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_rodeStag(string dest)
	{
	}

	// Token: 0x06000FAB RID: 4011 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_acquired(string item)
	{
	}

	// Token: 0x06000FAC RID: 4012 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_bought(string item)
	{
	}

	// Token: 0x06000FAD RID: 4013 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_quit()
	{
	}

	// Token: 0x06000FAE RID: 4014 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_rest()
	{
	}

	// Token: 0x06000FAF RID: 4015 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_upgradeNail()
	{
	}

	// Token: 0x06000FB0 RID: 4016 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_heartPiece()
	{
	}

	// Token: 0x06000FB1 RID: 4017 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_maxHealthUp()
	{
	}

	// Token: 0x06000FB2 RID: 4018 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_soulPiece()
	{
	}

	// Token: 0x06000FB3 RID: 4019 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_maxSoulUp()
	{
	}

	// Token: 0x06000FB4 RID: 4020 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_charmsChanged()
	{
	}

	// Token: 0x06000FB5 RID: 4021 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_charmEquipped(string charmName)
	{
	}

	// Token: 0x06000FB6 RID: 4022 RVA: 0x00003603 File Offset: 0x00001803
	public void StoryRecord_start()
	{
	}

	// Token: 0x06000FB7 RID: 4023 RVA: 0x0004B93D File Offset: 0x00049B3D
	public void AwardAchievement(string key)
	{
		//this.achievementHandler.AwardAchievementToPlayer(key);
	}

	// Token: 0x06000FB8 RID: 4024 RVA: 0x0004B94B File Offset: 0x00049B4B
	public void QueueAchievement(string key)
	{
		//this.achievementHandler.QueueAchievement(key);
	}

	// Token: 0x06000FB9 RID: 4025 RVA: 0x0004B959 File Offset: 0x00049B59
	public void AwardQueuedAchievements()
	{
		//this.achievementHandler.AwardQueuedAchievements();
	}

	// Token: 0x06000FBA RID: 4026 RVA: 0x0004B966 File Offset: 0x00049B66
	public bool IsAchievementAwarded(string key)
	{
		return false;
		//return this.achievementHandler.AchievementWasAwarded(key);
	}

	// Token: 0x06000FBB RID: 4027 RVA: 0x0004B974 File Offset: 0x00049B74
	public void ClearAllAchievements()
	{
		//this.achievementHandler.ResetAllAchievements();
	}

	// Token: 0x06000FBC RID: 4028 RVA: 0x0004B984 File Offset: 0x00049B84
	public void CheckCharmAchievements()
	{
		this.CountCharms();
		if (this.playerData.GetBool("hasCharm"))
		{
			this.AwardAchievement("CHARMED");
		}
		if (this.playerData.GetInt("charmsOwned") >= 20)
		{
			this.AwardAchievement("ENCHANTED");
		}
		if (this.playerData.GetBool("salubraBlessing"))
		{
			this.AwardAchievement("BLESSED");
		}
	}

	// Token: 0x06000FBD RID: 4029 RVA: 0x0004B9F0 File Offset: 0x00049BF0
	public void CheckGrubAchievements()
	{
		if (this.playerData.GetInt("grubsCollected") >= 23)
		{
			this.AwardAchievement("GRUBFRIEND");
		}
		if (this.playerData.GetInt("grubsCollected") >= 46)
		{
			this.AwardAchievement("METAMORPHOSIS");
		}
	}

	// Token: 0x06000FBE RID: 4030 RVA: 0x0004BA30 File Offset: 0x00049C30
	public void CheckStagStationAchievements()
	{
		if (this.playerData.GetInt("stationsOpened") >= 4)
		{
			this.AwardAchievement("STAG_STATION_HALF");
		}
	}

	// Token: 0x06000FBF RID: 4031 RVA: 0x0004BA50 File Offset: 0x00049C50
	public void CheckMapAchievement()
	{
		if (this.playerData.GetBool("mapCrossroads") && this.playerData.GetBool("mapGreenpath") && this.playerData.GetBool("mapFogCanyon") && this.playerData.GetBool("mapRoyalGardens") && this.playerData.GetBool("mapFungalWastes") && this.playerData.GetBool("mapCity") && this.playerData.GetBool("mapWaterways") && this.playerData.GetBool("mapMines") && this.playerData.GetBool("mapDeepnest") && this.playerData.GetBool("mapCliffs") && this.playerData.GetBool("mapOutskirts") && this.playerData.GetBool("mapRestingGrounds") && this.playerData.GetBool("mapAbyss"))
		{
			this.AwardAchievement("MAP");
		}
	}

	// Token: 0x06000FC0 RID: 4032 RVA: 0x0004BB68 File Offset: 0x00049D68
	public void CheckJournalAchievements()
	{
		//this.playerData.CountJournalEntries();
		if (this.playerData.GetInt("journalEntriesCompleted") >= this.playerData.GetInt("journalEntriesTotal"))
		{
			this.AwardAchievement("HUNTER_1");
		}
		if (this.playerData.GetBool("killedHunterMark"))
		{
			this.AwardAchievement("HUNTER_2");
		}
	}

	// Token: 0x06000FC1 RID: 4033 RVA: 0x0004BBCC File Offset: 0x00049DCC
	public void CheckAllAchievements()
	{
		/*if (!Platform.Current.IsFiringAchievementsFromSavesAllowed)
		{
			return;
		}*/
		this.CheckMapAchievement();
		this.CheckStagStationAchievements();
		this.CheckGrubAchievements();
		this.CheckCharmAchievements();
		this.CheckJournalAchievements();
		if (this.playerData.GetInt("MPReserveMax") > 0)
		{
			this.AwardAchievement("SOULFUL");
		}
		if (this.playerData.GetInt("MPReserveMax") == this.playerData.GetInt("MPReserveCap"))
		{
			this.AwardAchievement("WORLDSOUL");
		}
		if (this.playerData.GetInt("maxHealthBase") > 5)
		{
			this.AwardAchievement("PROTECTED");
		}
		if (this.playerData.GetInt("maxHealthBase") == this.playerData.GetInt("maxHealthCap"))
		{
			this.AwardAchievement("MASKED");
		}
		if (this.playerData.GetInt("dreamOrbs") >= 600)
		{
			this.AwardAchievement("ATTUNEMENT");
		}
		if (this.playerData.GetBool("dreamNailUpgraded"))
		{
			this.AwardAchievement("AWAKENING");
		}
		if (this.playerData.GetBool("mothDeparted"))
		{
			this.AwardAchievement("ASCENSION");
		}
		if (this.playerData.GetBool("hornet1Defeated"))
		{
			this.AwardAchievement("HORNET_1");
		}
		if (this.playerData.GetBool("hornetOutskirtsDefeated"))
		{
			this.AwardAchievement("HORNET_2");
		}
		if (this.playerData.GetBool("mageLordDefeated"))
		{
			this.AwardAchievement("SOUL_MASTER_DEFEAT");
		}
		if (this.playerData.GetBool("mageLordDreamDefeated"))
		{
			this.AwardAchievement("DREAM_SOUL_MASTER_DEFEAT");
		}
		if (this.playerData.GetBool("killedInfectedKnight"))
		{
			this.AwardAchievement("BROKEN_VESSEL");
		}
		if (this.playerData.GetBool("infectedKnightDreamDefeated"))
		{
			this.AwardAchievement("DREAM_BROKEN_VESSEL");
		}
		if (this.playerData.GetBool("killedDungDefender"))
		{
			this.AwardAchievement("DUNG_DEFENDER");
		}
		if (this.playerData.GetBool("falseKnightDreamDefeated"))
		{
			this.AwardAchievement("DREAM_FK");
		}
		if (this.playerData.GetBool("killedMantisLord"))
		{
			this.AwardAchievement("MANTIS_LORDS");
		}
		if (this.playerData.GetBool("killedJarCollector"))
		{
			this.AwardAchievement("COLLECTOR");
		}
		if (this.playerData.GetBool("killedTraitorLord"))
		{
			this.AwardAchievement("TRAITOR_LORD");
		}
		if (this.playerData.GetBool("killedWhiteDefender"))
		{
			this.AwardAchievement("WHITE_DEFENDER");
		}
		if (this.playerData.GetBool("killedGreyPrince"))
		{
			this.AwardAchievement("GREY_PRINCE");
		}
		if (this.playerData.GetBool("hegemolDefeated"))
		{
			this.AwardAchievement("BEAST");
		}
		if (this.playerData.GetBool("lurienDefeated"))
		{
			this.AwardAchievement("WATCHER");
		}
		if (this.playerData.GetBool("monomonDefeated"))
		{
			this.AwardAchievement("TEACHER");
		}
		if (this.playerData.GetBool("colosseumBronzeCompleted"))
		{
			this.AwardAchievement("COLOSSEUM_1");
		}
		if (this.playerData.GetBool("colosseumSilverCompleted"))
		{
			this.AwardAchievement("COLOSSEUM_2");
		}
		if (this.playerData.GetBool("colosseumGoldCompleted"))
		{
			this.AwardAchievement("COLOSSEUM_3");
		}
		if (this.playerData.GetBool("killedGrimm"))
		{
			this.AwardAchievement("GRIMM");
		}
		if (this.playerData.GetBool("defeatedNightmareGrimm"))
		{
			this.AwardAchievement("NIGHTMARE_GRIMM");
		}
		this.CheckBanishmentAchievement();
		if (this.playerData.GetBool("nailsmithKilled"))
		{
			this.AwardAchievement("NAILSMITH_KILL");
		}
		if (this.playerData.GetBool("nailsmithConvoArt"))
		{
			this.AwardAchievement("NAILSMITH_SPARE");
		}
		if (this.playerData.GetBool("mothDeparted"))
		{
			this.playerData.SetBoolSwappedArgs(true, "hasDreamGate");
		}
		if (this.playerData.GetBool("hasTramPass"))
		{
			this.AddToScenesVisited("Deepnest_26_b");
		}
		if (this.playerData.GetBool("slyRescued"))
		{
			this.AddToScenesVisited("Crossroads_04_b");
		}
		if (this.playerData.GetBool("gotCharm_32"))
		{
			this.AddToScenesVisited("Deepnest_East_14a");
		}
		/*if (this.playerData.GetBool("awardAllAchievements"))
		{
			this.achievementHandler.AwardAllAchievements();
		}*/
	}

	// Token: 0x06000FC2 RID: 4034 RVA: 0x0004C02E File Offset: 0x0004A22E
	public void CheckBanishmentAchievement()
	{
		if (this.playerData.GetBool("destroyedNightmareLantern"))
		{
			this.AwardAchievement("BANISHMENT");
		}
	}

	// Token: 0x06000FC3 RID: 4035 RVA: 0x0004C04D File Offset: 0x0004A24D
	public void SetStatusRecordInt(string key, int value)
	{
		//Platform.Current.EncryptedSharedData.SetInt(key, value);
	}

	// Token: 0x06000FC4 RID: 4036 RVA: 0x0004C060 File Offset: 0x0004A260
	public int GetStatusRecordInt(string key)
	{
		return 0;
		//return Platform.Current.EncryptedSharedData.GetInt(key, 0);
	}

	// Token: 0x06000FC5 RID: 4037 RVA: 0x0004C073 File Offset: 0x0004A273
	public void ResetStatusRecords()
	{
		//Platform.Current.EncryptedSharedData.DeleteKey("RecPermadeathMode");
	}

	// Token: 0x06000FC6 RID: 4038 RVA: 0x0004C089 File Offset: 0x0004A289
	public void SaveStatusRecords()
	{
		//Platform.Current.EncryptedSharedData.Save();
	}

	// Token: 0x06000FC7 RID: 4039 RVA: 0x0004C09A File Offset: 0x0004A29A
	public void SetState(GameState newState)
	{
		this.gameState = newState;
	}

	// Token: 0x06000FC8 RID: 4040 RVA: 0x0004C0A3 File Offset: 0x0004A2A3
	public void LoadScene(string destScene)
	{
		//destScene = ModHooks.BeforeSceneLoad(destScene);
		this.orig_LoadScene(destScene);
		//ModHooks.OnSceneChanged(destScene);
	}

	// Token: 0x06000FC9 RID: 4041 RVA: 0x0004C0BA File Offset: 0x0004A2BA
	public IEnumerator LoadSceneAdditive(string destScene)
	{
		Debug.Log("Loading " + destScene);
		//destScene = ModHooks.BeforeSceneLoad(destScene);
		this.tilemapDirty = true;
		this.startedOnThisScene = false;
		this.nextSceneName = destScene;
		this.waitForManualLevelStart = true;
		if (this.DestroyPersonalPools != null)
		{
			this.DestroyPersonalPools();
		}
		if (this.UnloadingLevel != null)
		{
			this.UnloadingLevel();
		}
		string exitingScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(destScene, LoadSceneMode.Additive);
		asyncOperation.allowSceneActivation = true;
		yield return asyncOperation;
		yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(exitingScene);
		//ModHooks.OnSceneChanged(destScene);
		this.RefreshTilemapInfo(destScene);
		if (this.IsUnloadAssetsRequired(exitingScene, destScene))
		{
			Debug.LogFormat(this, "Unloading assets due to zone transition", new object[0]);
			yield return Resources.UnloadUnusedAssets();
		}
		//GCManager.Collect();
		this.SetupSceneRefs(true);
		this.BeginScene();
		this.OnNextLevelReady();
		this.waitForManualLevelStart = false;
		Debug.Log("Done Loading " + destScene);
		yield break;
	}

	// Token: 0x06000FCA RID: 4042 RVA: 0x0004C0D0 File Offset: 0x0004A2D0
	public void OnNextLevelReady()
	{
		if (this.IsGameplayScene())
		{
			this.SetState(GameState.ENTERING_LEVEL);
			this.playerData.SetBoolSwappedArgs(false, "disablePause");
			//this.inputHandler.AllowPause();
			//this.inputHandler.StartAcceptingInput();
			this.EnterHero(true);
			this.UpdateUIStateFromGameState();
		}
	}

	// Token: 0x06000FCB RID: 4043 RVA: 0x0004C120 File Offset: 0x0004A320
	public void OnWillActivateFirstLevel()
	{
		this.orig_OnWillActivateFirstLevel();
		ModHooks.OnNewGame();
	}

	// Token: 0x06000FCC RID: 4044 RVA: 0x0004C12D File Offset: 0x0004A32D
	public IEnumerator LoadFirstScene()
	{
		yield return new WaitForEndOfFrame();
		this.OnWillActivateFirstLevel();
		this.LoadScene("Tutorial_01");
		ModHooks.OnNewGame();
		yield break;
	}

	// Token: 0x06000FCD RID: 4045 RVA: 0x0004C13C File Offset: 0x0004A33C
	public void LoadPermadeathUnlockScene()
	{
		if (this.GetStatusRecordInt("RecPermadeathMode") == 0)
		{
			this.LoadScene("PermaDeath_Unlock");
			return;
		}
		base.StartCoroutine(this.ReturnToMainMenu(GameManager.ReturnToMainMenuSaveModes.SaveAndContinueOnFail, null));
	}

	// Token: 0x06000FCE RID: 4046 RVA: 0x0004C166 File Offset: 0x0004A366
	public void LoadMrMushromScene()
	{
		if (this.playerData.GetInt("mrMushroomState") >= 8)
		{
			this.LoadScene("Cinematic_MrMushroom");
			return;
		}
		this.LoadScene("End_Game_Completion");
	}

	// Token: 0x06000FCF RID: 4047 RVA: 0x0004C192 File Offset: 0x0004A392
	public void LoadOpeningCinematic()
	{
		this.SetState(GameState.CUTSCENE);
		this.LoadScene("Intro_Cutscene");
	}

	// Token: 0x06000FD0 RID: 4048 RVA: 0x0004C1A8 File Offset: 0x0004A3A8
	private void PositionHeroAtSceneEntrance()
	{
		Vector2 position = this.FindEntryPoint(this.entryGateName, default(Scene)) ?? new Vector2(-20000f, 20000f);
		if (this.hero_ctrl != null)
		{
			this.hero_ctrl.transform.SetPosition2D(position);
		}
	}

	// Token: 0x06000FD1 RID: 4049 RVA: 0x0004C20C File Offset: 0x0004A40C
	private Vector2? FindEntryPoint(string entryPointName, Scene filterScene)
	{
		if (this.RespawningHero)
		{
			Transform transform = this.hero_ctrl.LocateSpawnPoint();
			if (transform != null)
			{
				return new Vector2?(transform.transform.position);
			}
			return null;
		}
		else
		{
			if (this.hazardRespawningHero)
			{
				return new Vector2?(this.playerData.GetVector3("hazardRespawnLocation"));
			}
			if (this.entryGateName == "dreamGate")
			{
				return new Vector2?(new Vector2(this.playerData.GetFloat("dreamGateX"), this.playerData.GetFloat("dreamGateY")));
			}
			TransitionPoint transitionPoint = this.FindTransitionPoint(entryPointName, filterScene, true);
			if (transitionPoint != null)
			{
				return new Vector2?((Vector2)transitionPoint.transform.position + transitionPoint.entryOffset);
			}
			return null;
		}
	}

	// Token: 0x06000FD2 RID: 4050 RVA: 0x0004C2F4 File Offset: 0x0004A4F4
	private TransitionPoint FindTransitionPoint(string entryPointName, Scene filterScene, bool fallbackToAnyAvailable)
	{
		List<TransitionPoint> transitionPoints = TransitionPoint.TransitionPoints;
		for (int i = 0; i < transitionPoints.Count; i++)
		{
			TransitionPoint transitionPoint = transitionPoints[i];
			if (transitionPoint.name == entryPointName && (!filterScene.IsValid() || transitionPoint.gameObject.scene == filterScene))
			{
				return transitionPoint;
			}
		}
		if (fallbackToAnyAvailable && transitionPoints.Count > 0)
		{
			return transitionPoints[0];
		}
		return null;
	}

	// Token: 0x06000FD3 RID: 4051 RVA: 0x0004C364 File Offset: 0x0004A564
	private void EnterHero(bool additiveGateSearch = false)
	{
		if (this.entryGateName == "door_dreamReturn" && !string.IsNullOrEmpty(this.playerData.GetString("bossReturnEntryGate")))
		{
			if (this.GetCurrentMapZone() == MapZone.GODS_GLORY.ToString())
			{
				this.entryGateName = this.playerData.GetString("bossReturnEntryGate");
			}
			this.playerData.SetStringSwappedArgs(string.Empty, "bossReturnEntryGate");
		}
		if (this.RespawningHero)
		{
			if (this.needFirstFadeIn)
			{
				base.StartCoroutine(this.FadeSceneInWithDelay(0.3f));
				this.needFirstFadeIn = false;
			}
			base.StartCoroutine(this.hero_ctrl.Respawn());
			this.FinishedEnteringScene();
			this.RespawningHero = false;
			return;
		}
		if (this.hazardRespawningHero)
		{
			base.StartCoroutine(this.hero_ctrl.HazardRespawn());
			this.FinishedEnteringScene();
			this.hazardRespawningHero = false;
			return;
		}
		if (this.entryGateName == "dreamGate")
		{
			this.hero_ctrl.EnterSceneDreamGate();
			return;
		}
		if (this.startedOnThisScene)
		{
			if (this.IsGameplayScene())
			{
				this.FinishedEnteringScene();
				this.FadeSceneIn();
			}
			return;
		}
		this.SetState(GameState.ENTERING_LEVEL);
		if (string.IsNullOrEmpty(this.entryGateName))
		{
			Debug.LogError("No entry gate has been defined in the Game Manager, unable to move hero into position.");
			this.FinishedEnteringScene();
			return;
		}
		if (additiveGateSearch)
		{
			if (this.verboseMode)
			{
				Debug.Log("Searching for entry gate " + this.entryGateName + " in the next scene: " + this.nextSceneName);
			}
			foreach (GameObject gameObject in UnityEngine.SceneManagement.SceneManager.GetSceneByName(this.nextSceneName).GetRootGameObjects())
			{
				TransitionPoint component = gameObject.GetComponent<TransitionPoint>();
				if (component != null && component.name == this.entryGateName)
				{
					if (this.verboseMode)
					{
						Debug.Log("SUCCESS - Found as root object");
					}
					base.StartCoroutine(this.hero_ctrl.EnterScene(component, this.entryDelay));
					return;
				}
				if (gameObject.name == "_Transition Gates")
				{
					TransitionPoint[] componentsInChildren = gameObject.GetComponentsInChildren<TransitionPoint>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						if (componentsInChildren[j].name == this.entryGateName)
						{
							if (this.verboseMode)
							{
								Debug.Log("SUCCESS - Found in _Transition Gates folder");
							}
							base.StartCoroutine(this.hero_ctrl.EnterScene(componentsInChildren[j], this.entryDelay));
							return;
						}
					}
				}
				TransitionPoint[] componentsInChildren2 = gameObject.GetComponentsInChildren<TransitionPoint>();
				for (int k = 0; k < componentsInChildren2.Length; k++)
				{
					if (componentsInChildren2[k].name == this.entryGateName)
					{
						if (this.verboseMode)
						{
							Debug.Log("SUCCESS - Found as a child of a random scene object, can't win em all");
						}
						base.StartCoroutine(this.hero_ctrl.EnterScene(componentsInChildren2[k], this.entryDelay));
						return;
					}
				}
			}
			Debug.LogError("Searching in next scene for TransitionGate failed.");
			return;
		}
		GameObject gameObject2 = GameObject.Find(this.entryGateName);
		if (gameObject2 != null)
		{
			TransitionPoint component2 = gameObject2.GetComponent<TransitionPoint>();
			base.StartCoroutine(this.hero_ctrl.EnterScene(component2, this.entryDelay));
			return;
		}
		Debug.LogError(string.Concat(new string[]
		{
			"No entry point found with the name \"",
			this.entryGateName,
			"\" in this scene (",
			this.sceneName,
			"). Unable to move hero into position, trying alternative gates..."
		}));
		TransitionPoint[] array = UnityEngine.Object.FindObjectsOfType<TransitionPoint>();
		if (array != null)
		{
			base.StartCoroutine(this.hero_ctrl.EnterScene(array[0], this.entryDelay));
			return;
		}
		Debug.LogError("Could not find any gates in this scene. Trying last ditch spawn...");
		//this.hero_ctrl.transform.SetPosition2D((float)this.tilemap.width / 2f, (float)this.tilemap.height / 2f);
		this.hero_ctrl.EnterSceneDreamGate();
	}

	// Token: 0x06000FD4 RID: 4052 RVA: 0x0004C732 File Offset: 0x0004A932
	public void FinishedEnteringScene()
	{
		this.SetState(GameState.PLAYING);
		this.entryDelay = 0f;
		this.hasFinishedEnteringScene = true;
		if (this.OnFinishedEnteringScene != null)
		{
			this.OnFinishedEnteringScene();
		}
	}

	// Token: 0x06000FD5 RID: 4053 RVA: 0x0004C760 File Offset: 0x0004A960
	private void SetupGameRefs()
	{
		this.playerData = PlayerData.instance;
		//this.sceneData = SceneData.instance;
		this.gameCams = GameCameras.instance;
		this.cameraCtrl = this.gameCams.cameraController;
		//this.gameSettings = new GameSettings();
		//this.inputHandler = base.GetComponent<InputHandler>();
		//this.achievementHandler = base.GetComponent<AchievementHandler>();
		/*if (GameManager._spawnedInControlManager == null)
		{
			GameManager._spawnedInControlManager = UnityEngine.Object.Instantiate<InControlManager>(this.inControlManagerPrefab);
			UnityEngine.Object.DontDestroyOnLoad(GameManager._spawnedInControlManager);
		}*/
		//this.inventoryFSM = this.gameCams.gameObject.transform.Find("HudCamera").gameObject.transform.Find("Inventory").gameObject.GetComponent<PlayMakerFSM>();
		/*if (AchievementPopupHandler.Instance)
		{
			AchievementPopupHandler.Instance.Setup(this.achievementHandler);
		}*/
		//Platform.Current.AdjustGraphicsSettings(this.gameSettings);
		/*if (this.inputHandler == null)
		{
			Debug.LogError("Couldn't find InputHandler component.");
		}
		if (this.achievementHandler == null)
		{
			Debug.LogError("Couldn't find AchievementHandler component.");
		}*/
		UnityEngine.SceneManagement.SceneManager.activeSceneChanged += this.LevelActivated;
		//Platform.Current.SetDisengageHandler(this);
	}

	// Token: 0x06000FD6 RID: 4054 RVA: 0x0004C8A0 File Offset: 0x0004AAA0
	public void SetupSceneRefs(bool refreshTilemapInfo)
	{
		this.orig_SetupSceneRefs(refreshTilemapInfo);
		if (this.IsGameplayScene())
		{
			/*tk2dSpriteAnimator component = GameCameras.instance.soulOrbFSM.gameObject.transform.Find("SoulOrb_fill").gameObject.transform.Find("Liquid").gameObject.GetComponent<tk2dSpriteAnimator>();
			component.GetClipByName("Fill").fps = 15.749999f;
			component.GetClipByName("Idle").fps = 10.5f;
			component.GetClipByName("Shrink").fps = 15.749999f;
			component.GetClipByName("Drain").fps = 31.499998f;*/
		}
	}

	// Token: 0x06000FD7 RID: 4055 RVA: 0x0004C94E File Offset: 0x0004AB4E
	public void SetupHeroRefs()
	{
		this.hero_ctrl = HeroController.instance;
		//this.heroLight = GameObject.FindGameObjectWithTag("HeroLightMain").GetComponent<SpriteRenderer>();
	}

	// Token: 0x06000FD8 RID: 4056 RVA: 0x0004C970 File Offset: 0x0004AB70
	public void BeginScene()
	{
		//this.inputHandler.SceneInit();
		//this.ui.SceneInit();
		if (this.hero_ctrl)
		{
			this.hero_ctrl.SceneInit();
		}
		this.gameCams.SceneInit();
		if (this.IsMenuScene())
		{
			this.SetState(GameState.MAIN_MENU);
			this.UpdateUIStateFromGameState();
			//Platform.Current.SetSocialPresence("IN_MENU", true);
			return;
		}
		if (this.IsGameplayScene())
		{
			if ((!Application.isEditor && !Debug.isDebugBuild) || Time.renderedFrameCount > 3)
			{
				this.PositionHeroAtSceneEntrance();
			}
			if (this.sm != null)
			{
				//Platform.Current.SetSocialPresence("EXPLORING_" + this.sm.mapZone.ToString(), true);
				return;
			}
		}
		else
		{
			if (this.IsNonGameplayScene())
			{
				this.SetState(GameState.CUTSCENE);
				this.UpdateUIStateFromGameState();
				return;
			}
			Debug.LogError("GM - Scene type is not set to a standard scene type.");
			this.UpdateUIStateFromGameState();
		}
	}

	// Token: 0x06000FD9 RID: 4057 RVA: 0x0004CA68 File Offset: 0x0004AC68
	private void UpdateUIStateFromGameState()
	{
		/*if (this.ui != null)
		{
			this.ui.SetUIStartState(this.gameState);
			return;
		}
		this.ui = UnityEngine.Object.FindObjectOfType<UIManager>();
		if (this.ui != null)
		{
			this.ui.SetUIStartState(this.gameState);
			return;
		}
		Debug.LogError("GM: Could not find the UI manager in this scene.");*/
	}

	// Token: 0x06000FDA RID: 4058 RVA: 0x0004CACA File Offset: 0x0004ACCA
	public void SkipCutscene()
	{
		base.StartCoroutine(this.SkipCutsceneNoMash());
	}

	// Token: 0x06000FDB RID: 4059 RVA: 0x0004CAD9 File Offset: 0x0004ACD9
	private IEnumerator SkipCutsceneNoMash()
	{
		/*if (this.gameState == GameState.CUTSCENE)
		{
			this.ui.HideCutscenePrompt();
			CinematicPlayer cinematicPlayer = UnityEngine.Object.FindObjectOfType<CinematicPlayer>();
			if (cinematicPlayer != null)
			{
				yield return this.StartCoroutine(cinematicPlayer.SkipVideo());
				this.inputHandler.skippingCutscene = false;
				yield break;
			}
			CutsceneHelper cutsceneHelper = UnityEngine.Object.FindObjectOfType<CutsceneHelper>();
			if (cutsceneHelper != null)
			{
				yield return this.StartCoroutine(cutsceneHelper.SkipCutscene());
				this.inputHandler.skippingCutscene = false;
				yield break;
			}
			OpeningSequence openingSequence = UnityEngine.Object.FindObjectOfType<OpeningSequence>();
			if (openingSequence != null)
			{
				yield return this.StartCoroutine(openingSequence.Skip());
				this.inputHandler.skippingCutscene = false;
				yield break;
			}
			StagTravel stagTravel = UnityEngine.Object.FindObjectOfType<StagTravel>();
			if (stagTravel != null)
			{
				yield return this.StartCoroutine(stagTravel.Skip());
				this.inputHandler.skippingCutscene = false;
				yield break;
			}
			Debug.LogError("Unable to skip, please ensure there is a CinematicPlayer or CutsceneHelper in this scene.");
		}*/
		yield break;
	}

	// Token: 0x06000FDC RID: 4060 RVA: 0x0004CAE8 File Offset: 0x0004ACE8
	public void NoLongerFirstGame()
	{
		if (this.playerData.GetBool("isFirstGame"))
		{
			this.playerData.SetBoolSwappedArgs(false, "isFirstGame");
		}
	}

	// Token: 0x06000FDD RID: 4061 RVA: 0x0004CB10 File Offset: 0x0004AD10
	private void SetupStatusModifiers()
	{
		/*if (this.gameConfig.clearRecordsOnStart)
		{
			this.ResetStatusRecords();
		}
		if (this.gameConfig.unlockPermadeathMode)
		{
			this.SetStatusRecordInt("RecPermadeathMode", 1);
		}
		if (this.gameConfig.unlockBossRushMode)
		{
			this.SetStatusRecordInt("RecBossRushMode", 1);
		}
		if (this.gameConfig.clearPreferredLanguageSetting)
		{
			Platform.Current.SharedData.DeleteKey("GameLangSet");
		}
		if (this.gameSettings.CommandArgumentUsed("-forcelang"))
		{
			Debug.Log("== Language option forced on by command argument.");
			this.gameConfig.hideLanguageOption = true;
		}*/
	}

	// Token: 0x06000FDE RID: 4062 RVA: 0x0004CBAB File Offset: 0x0004ADAB
	public void MatchBackerCreditsSetting()
	{
		/*if (this.gameSettings.backerCredits > 0)
		{
			this.playerData.SetBoolSwappedArgs(true, "backerCredits");
			return;
		}*/
		this.playerData.SetBoolSwappedArgs(false, "backerCredits");
	}

	// Token: 0x06000FDF RID: 4063 RVA: 0x0004CBDE File Offset: 0x0004ADDE
	public void RefreshLocalization()
	{
		if (this.RefreshLanguageText != null)
		{
			this.RefreshLanguageText();
		}
	}

	// Token: 0x06000FE0 RID: 4064 RVA: 0x0004CBF3 File Offset: 0x0004ADF3
	public void RefreshParticleSystems()
	{
		if (this.RefreshParticleLevel != null)
		{
			this.RefreshParticleLevel();
		}
	}

	// Token: 0x06000FE1 RID: 4065 RVA: 0x00003603 File Offset: 0x00001803
	public void ApplyNativeInput()
	{
	}

	// Token: 0x06000FE2 RID: 4066 RVA: 0x0004CC08 File Offset: 0x0004AE08
	public void EnablePermadeathMode()
	{
		this.SetStatusRecordInt("RecPermadeathMode", 1);
	}

	// Token: 0x06000FE3 RID: 4067 RVA: 0x0004CC16 File Offset: 0x0004AE16
	public string GetCurrentMapZone()
	{
		return this.sm.mapZone.ToString();
	}

	// Token: 0x06000FE4 RID: 4068 RVA: 0x0004CC2E File Offset: 0x0004AE2E
	public float GetSceneWidth()
	{
		if (this.IsGameplayScene())
		{
			return this.sceneWidth;
		}
		return 0f;
	}

	// Token: 0x06000FE5 RID: 4069 RVA: 0x0004CC44 File Offset: 0x0004AE44
	public float GetSceneHeight()
	{
		if (this.IsGameplayScene())
		{
			return this.sceneHeight;
		}
		return 0f;
	}

	// Token: 0x06000FE6 RID: 4070 RVA: 0x0004CC5A File Offset: 0x0004AE5A
	public GameObject GetSceneManager()
	{
		return this.sm.gameObject;
	}

	// Token: 0x06000FE7 RID: 4071 RVA: 0x0004CC67 File Offset: 0x0004AE67
	public string GetFormattedMapZoneString(MapZone mapZone)
	{
		return null;
		//return Language.Get(mapZone.ToString(), "Map Zones");
	}

	// Token: 0x06000FE8 RID: 4072 RVA: 0x0004CC80 File Offset: 0x0004AE80
	public void UpdateSceneName()
	{
		this.sceneName = GameManager.GetBaseSceneName(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
	}

	// Token: 0x06000FE9 RID: 4073 RVA: 0x0004CCA8 File Offset: 0x0004AEA8
	public static string GetBaseSceneName(string fullSceneName)
	{
		for (int i = 0; i < GameManager.SubSceneNameSuffixes.Length; i++)
		{
			string text = GameManager.SubSceneNameSuffixes[i];
			if (fullSceneName.EndsWith(text, StringComparison.InvariantCultureIgnoreCase))
			{
				return fullSceneName.Substring(0, fullSceneName.Length - text.Length);
			}
		}
		return fullSceneName;
	}

	// Token: 0x06000FEA RID: 4074 RVA: 0x0004CCEF File Offset: 0x0004AEEF
	public string GetSceneNameString()
	{
		this.UpdateSceneName();
		return this.sceneName;
	}

	// Token: 0x06000FEB RID: 4075 RVA: 0x0004CCFD File Offset: 0x0004AEFD
	/*private static tk2dTileMap GetTileMap(GameObject gameObject)
	{
		if (gameObject.CompareTag("TileMap"))
		{
			return gameObject.GetComponent<tk2dTileMap>();
		}
		return null;
	}*/

	static Type FindType(string assemblyName, string typeName)
	{
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (asm.GetName().Name == assemblyName)
			{
				return asm.GetType(typeName);
			}
		}
		return null;
	}

	// Token: 0x06000FEC RID: 4076 RVA: 0x0004CD14 File Offset: 0x0004AF14
	public void RefreshTilemapInfo(string targetScene)
	{
		Debug.Log("Target Scene = " + targetScene);
		var sceneManagerType = FindType("WeaverCore", "WeaverCore.Components.WeaverSceneManager");
		var sceneDimProp = sceneManagerType.GetProperty("SceneDimensions");

		for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
		{
			var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
			if (scene.name == targetScene)
			{
				foreach (var gm in scene.GetRootGameObjects())
				{
					Debug.Log("Root Object = " + gm.gameObject);
					Component manager = gm.GetComponent(sceneManagerType);
					if (manager != null)
					{
						Debug.Log("Found Scene Manager = " + gm.name);
						Rect dimensions = (Rect)sceneDimProp.GetValue(manager);
						SceneDimensions = dimensions;
						sceneWidth = dimensions.width;
						sceneHeight = dimensions.height;
						return;
					}
				}
			}
		}

		foreach (Component manager in FindObjectsOfType(sceneManagerType))
		{
			GameObject gm = manager.gameObject;
			if (gm.scene.name == targetScene)
			{
				Rect dimensions = (Rect)sceneDimProp.GetValue(manager);
				SceneDimensions = dimensions;
				sceneWidth = dimensions.width;
				sceneHeight = dimensions.height;
				return;
			}
		}

		var placeHolderObject = new GameObject("PLACEHOLDER SCENE MANAGER");

		var placeholderManager = placeHolderObject.AddComponent(sceneManagerType);

		Rect dim = (Rect)sceneDimProp.GetValue(placeholderManager);
		SceneDimensions = dim;
		sceneWidth = dim.width;
		sceneHeight = dim.height;

		//If no WeaverSceneManager was found
		//Debug.LogError("No WeaverSceneManager was found in the scene!");
		/*for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
		{
			Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
		}*/
		//TODO - FIND WORKAROUND
		/*tk2dTileMap tk2dTileMap = null;
		int num = 0;
		while (tk2dTileMap == null && num < UnityEngine.SceneManagement.SceneManager.sceneCount)
		{
			Scene sceneAt = UnityEngine.SceneManagement.SceneManager.GetSceneAt(num);
			if (string.IsNullOrEmpty(targetScene) || !(sceneAt.name != targetScene))
			{
				GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
				int num2 = 0;
				while (tk2dTileMap == null && num2 < rootGameObjects.Length)
				{
					tk2dTileMap = GameManager.GetTileMap(rootGameObjects[num2]);
					num2++;
				}
			}
			num++;
		}
		if (tk2dTileMap == null)
		{
			Debug.LogErrorFormat("Using fallback 1 to find tilemap. Scene {0} requires manual fixing.", new object[]
			{
				targetScene
			});
			GameObject[] array = GameObject.FindGameObjectsWithTag("TileMap");
			int num3 = 0;
			while (tk2dTileMap == null && num3 < array.Length)
			{
				tk2dTileMap = array[num3].GetComponent<tk2dTileMap>();
				num3++;
			}
		}
		if (tk2dTileMap == null)
		{
			Debug.LogErrorFormat("Using fallback 2 to find tilemap. Scene {0} requires manual fixing.", new object[]
			{
				targetScene
			});
			GameObject gameObject = GameObject.Find("TileMap");
			if (gameObject != null)
			{
				tk2dTileMap = GameManager.GetTileMap(gameObject);
			}
		}
		if (tk2dTileMap == null)
		{
			Debug.LogErrorFormat("Failed to find tilemap in {0} entirely.", new object[]
			{
				targetScene
			});
			return;
		}
		this.tilemap = tk2dTileMap;
		this.sceneWidth = (float)this.tilemap.width;
		this.sceneHeight = (float)this.tilemap.height;*/
	}

	// Token: 0x06000FED RID: 4077 RVA: 0x0004CE5B File Offset: 0x0004B05B
	public void SaveLevelState()
	{
		if (this.SavePersistentObjects != null)
		{
			this.SavePersistentObjects();
		}
	}

	// Token: 0x06000FEE RID: 4078 RVA: 0x0004CE70 File Offset: 0x0004B070
	public void ResetSemiPersistentItems()
	{
		/*if (this.ResetSemiPersistentObjects != null)
		{
			this.ResetSemiPersistentObjects();
		}
		this.sceneData.ResetSemiPersistentItems();*/
	}

	// Token: 0x06000FEF RID: 4079 RVA: 0x0004CE90 File Offset: 0x0004B090
	public bool IsMenuScene()
	{
		this.UpdateSceneName();
		return this.sceneName == "Menu_Title";
	}

	// Token: 0x06000FF0 RID: 4080 RVA: 0x0004CEAD File Offset: 0x0004B0AD
	public bool IsTitleScreenScene()
	{
		this.UpdateSceneName();
		return string.Compare(this.sceneName, "Title_Screens", true) == 0;
	}

	// Token: 0x06000FF1 RID: 4081 RVA: 0x0004CECB File Offset: 0x0004B0CB
	public bool IsGameplayScene()
	{
		this.UpdateSceneName();
		return !this.IsNonGameplayScene();
	}

	// Token: 0x06000FF2 RID: 4082 RVA: 0x0004CEE0 File Offset: 0x0004B0E0
	public bool IsNonGameplayScene()
	{
		this.UpdateSceneName();
		return this.IsCinematicScene() || this.sceneName == "Knight Pickup" || this.sceneName == "Pre_Menu_Intro" || this.sceneName == "Menu_Title" || this.sceneName == "End_Credits" || this.sceneName == "Menu_Credits" || this.sceneName == "Cutscene_Boss_Door" || this.sceneName == "PermaDeath_Unlock" || this.sceneName == "GG_Unlock" || this.sceneName == "GG_End_Sequence" || this.sceneName == "End_Game_Completion" || this.sceneName == "BetaEnd" || this.sceneName == "PermaDeath" || this.sceneName == "GG_Entrance_Cutscene" || this.sceneName == "GG_Boss_Door_Entrance";
	}

	// Token: 0x06000FF3 RID: 4083 RVA: 0x0004D020 File Offset: 0x0004B220
	public bool IsCinematicScene()
	{
		this.UpdateSceneName();
		return this.sceneName == "Intro_Cutscene_Prologue" || this.sceneName == "Opening_Sequence" || this.sceneName == "Prologue_Excerpt" || this.sceneName == "Intro_Cutscene" || this.sceneName == "Cinematic_Stag_travel" || this.sceneName == "PermaDeath" || this.sceneName == "Cinematic_Ending_A" || this.sceneName == "Cinematic_Ending_B" || this.sceneName == "Cinematic_Ending_C" || this.sceneName == "Cinematic_Ending_D" || this.sceneName == "Cinematic_Ending_E" || this.sceneName == "Cinematic_MrMushroom" || this.sceneName == "BetaEnd";
	}

	// Token: 0x06000FF4 RID: 4084 RVA: 0x0004D138 File Offset: 0x0004B338
	public bool IsStagTravelScene()
	{
		this.UpdateSceneName();
		return this.sceneName == "Cinematic_Stag_travel";
	}

	// Token: 0x06000FF5 RID: 4085 RVA: 0x0004D155 File Offset: 0x0004B355
	public bool IsBetaEndScene()
	{
		this.UpdateSceneName();
		return this.sceneName == "BetaEnd";
	}

	// Token: 0x06000FF6 RID: 4086 RVA: 0x0004D172 File Offset: 0x0004B372
	public bool IsTutorialScene()
	{
		this.UpdateSceneName();
		return this.sceneName == "Tutorial_01";
	}

	// Token: 0x06000FF7 RID: 4087 RVA: 0x0004D18F File Offset: 0x0004B38F
	public bool IsTestScene()
	{
		this.UpdateSceneName();
		return this.sceneName.Contains("test");
	}

	// Token: 0x06000FF8 RID: 4088 RVA: 0x0004D1AC File Offset: 0x0004B3AC
	public bool IsBossDoorScene()
	{
		this.UpdateSceneName();
		return this.sceneName == "Cutscene_Boss_Door";
	}

	// Token: 0x06000FF9 RID: 4089 RVA: 0x0004D1CC File Offset: 0x0004B3CC
	public bool ShouldKeepHUDCameraActive()
	{
		this.UpdateSceneName();
		return this.sceneName == "GG_Entrance_Cutscene" || this.sceneName == "GG_Boss_Door_Entrance" || this.sceneName == "GG_End_Sequence" || this.sceneName == "Cinematic_Ending_D";
	}

	// Token: 0x06000FFA RID: 4090 RVA: 0x0004D230 File Offset: 0x0004B430
	private static string GetSceneZoneName(string str)
	{
		int num = str.IndexOf('_');
		if (num <= 0)
		{
			return str;
		}
		int num2 = num - 1;
		while (num2 > 0 && char.IsDigit(str[num2]))
		{
			num2--;
		}
		return str.Substring(0, num2 + 1);
	}

	// Token: 0x06000FFB RID: 4091 RVA: 0x0004D274 File Offset: 0x0004B474
	private static int CountBits(int val)
	{
		int num = 0;
		while (val != 0)
		{
			if ((val & 1) != 0)
			{
				num++;
			}
			val >>= 1;
		}
		return num;
	}

	// Token: 0x06000FFC RID: 4092 RVA: 0x0004D29C File Offset: 0x0004B49C
	public bool IsUnloadAssetsRequired(string sourceSceneName, string destinationSceneName)
	{
		/*WorldInfo.SceneInfo sceneInfo = this.worldInfo.GetSceneInfo(sourceSceneName);
		WorldInfo.SceneInfo sceneInfo2 = this.worldInfo.GetSceneInfo(destinationSceneName);
		bool flag = false;
		if (sceneInfo != null && sceneInfo2 != null)
		{
			bool flag2 = (sceneInfo.ZoneTags & 1) == 0;
			bool flag3 = (sceneInfo2.ZoneTags & 1) == 0;
			if (GameManager.CountBits(sceneInfo2.ZoneTags) == 1 && sceneInfo2.ZoneTags != sceneInfo.ZoneTags && !flag2 && !flag3)
			{
				flag = true;
			}
		}
		if (Application.isEditor)
		{
			if (flag)
			{
				Debug.LogFormat(this, "This transition is a zone transition, which would have a lower memory threshold.", Array.Empty<object>());
			}
			return false;
		}
		long memoryUsage = GCManager.GetMemoryUsage();
		if (memoryUsage > (long)((ulong)-1673527296))
		{
			Debug.LogFormat(this, "Memory exceeds high memory limit ({0}/{1}).", new object[]
			{
				memoryUsage,
				(long)((ulong)-1673527296)
			});
			return true;
		}
		if (flag && memoryUsage > (long)((ulong)-2092957696))
		{
			Debug.LogFormat(this, "Memory exceeds low memory limit ({0}/{1}), and this is a zone transition.", new object[]
			{
				memoryUsage,
				(long)((ulong)-2092957696)
			});
			return true;
		}*/
		return false;
	}

	// Token: 0x06000FFD RID: 4093 RVA: 0x0004D397 File Offset: 0x0004B597
	public void HasSaveFile(int saveSlot, Action<bool> callback)
	{
		//Platform.Current.IsSaveSlotInUse(saveSlot, callback);
	}

	// Token: 0x06000FFE RID: 4094 RVA: 0x0004D3A5 File Offset: 0x0004B5A5
	public void SaveGame()
	{
		this.SaveGame(delegate (bool didSave)
		{
		});
	}

	// Token: 0x06000FFF RID: 4095 RVA: 0x0004D3CC File Offset: 0x0004B5CC
	private void ShowSaveIcon()
	{
		/*UIManager instance = UIManager.instance;
		if (instance != null && this.saveIconShowCounter == 0)
		{
			CheckpointSprite checkpointSprite = instance.checkpointSprite;
			if (checkpointSprite != null)
			{
				checkpointSprite.Show();
			}
		}
		this.saveIconShowCounter++;*/
	}

	// Token: 0x06001000 RID: 4096 RVA: 0x0004D414 File Offset: 0x0004B614
	private void HideSaveIcon()
	{
		/*this.saveIconShowCounter--;
		UIManager instance = UIManager.instance;
		if (instance != null && this.saveIconShowCounter == 0)
		{
			CheckpointSprite checkpointSprite = instance.checkpointSprite;
			if (checkpointSprite != null)
			{
				checkpointSprite.Hide();
			}
		}*/
	}

	// Token: 0x06001001 RID: 4097 RVA: 0x0004D45C File Offset: 0x0004B65C
	public void SaveGame(Action<bool> callback)
	{
		this.SaveGame(this.profileID, callback);
	}

	// Token: 0x06001002 RID: 4098 RVA: 0x0004D46B File Offset: 0x0004B66B
	private void ResetGameTimer()
	{
		this.sessionPlayTimer = 0f;
		this.sessionStartTime = this.playerData.GetFloat("playTime");
	}

	// Token: 0x06001003 RID: 4099 RVA: 0x0004D48E File Offset: 0x0004B68E
	public void IncreaseGameTimer(ref float timer)
	{
		if ((this.gameState == GameState.PLAYING || this.gameState == GameState.ENTERING_LEVEL || this.gameState == GameState.EXITING_LEVEL) && Time.unscaledDeltaTime < 1f)
		{
			timer += Time.unscaledDeltaTime;
		}
	}

	// Token: 0x06001004 RID: 4100 RVA: 0x0004D4C4 File Offset: 0x0004B6C4
	public void SaveGame(int saveSlot, Action<bool> callback)
	{
		/*if (saveSlot >= 0)
		{
			this.SaveLevelState();
			if (!this.gameConfig.disableSaveGame)
			{
				this.ShowSaveIcon();
				if (this.achievementHandler != null)
				{
					this.achievementHandler.FlushRecordsToDisk();
				}
				else
				{
					Debug.LogError("Error saving achievements (PlayerAchievements is null)");
				}
				if (this.playerData != null)
				{
					this.playerData.playTime += this.sessionPlayTimer;
					this.ResetGameTimer();
					this.playerData.version = "1.5.72.11824";
					this.playerData.profileID = saveSlot;
					this.playerData.CountGameCompletion();
				}
				else
				{
					Debug.LogError("Error updating PlayerData before save (PlayerData is null)");
				}
				try
				{
					SaveGameData saveGameData = new SaveGameData(this.playerData, this.sceneData);
					ModHooks.OnBeforeSaveGameSave(saveGameData);
					if (this.moddedData == null)
					{
						this.moddedData = new ModSavegameData();
					}
					ModHooks.OnSaveLocalSettings(this.moddedData);
					try
					{
						string text = GameManager.ModdedSavePath(saveSlot);
						string value = JsonConvert.SerializeObject(this.moddedData, Formatting.Indented, new JsonSerializerSettings
						{
							ContractResolver = ShouldSerializeContractResolver.Instance,
							TypeNameHandling = TypeNameHandling.Auto,
							Converters = JsonConverterTypes.ConverterTypes
						});
						if (File.Exists(text + ".bak"))
						{
							File.Delete(text + ".bak");
						}
						if (File.Exists(text))
						{
							File.Move(text, text + ".bak");
						}
						using (FileStream fileStream = File.Create(text))
						{
							using (StreamWriter streamWriter = new StreamWriter(fileStream))
							{
								streamWriter.Write(value);
							}
						}
					}
					catch (Exception message)
					{
						Modding.Logger.APILogger.LogError(message);
					}
					string text2 = null;
					try
					{
						text2 = JsonConvert.SerializeObject(saveGameData, Formatting.Indented, new JsonSerializerSettings
						{
							ContractResolver = ShouldSerializeContractResolver.Instance,
							TypeNameHandling = TypeNameHandling.Auto,
							Converters = JsonConverterTypes.ConverterTypes
						});
					}
					catch (Exception message2)
					{
						Modding.Logger.LogError("Failed to serialize save using Json.NET, trying fallback.");
						Modding.Logger.APILogger.LogError(message2);
						text2 = JsonUtility.ToJson(saveGameData);
					}
					if (this.gameConfig.useSaveEncryption && !Platform.Current.IsFileSystemProtected)
					{
						string graph = Encryption.Encrypt(text2);
						BinaryFormatter binaryFormatter = new BinaryFormatter();
						MemoryStream memoryStream = new MemoryStream();
						binaryFormatter.Serialize(memoryStream, graph);
						byte[] binary = memoryStream.ToArray();
						memoryStream.Close();
						Platform.Current.WriteSaveSlot(saveSlot, binary, delegate (bool didSave)
						{
							this.HideSaveIcon();
							callback(didSave);
						});
					}
					else
					{
						Platform.Current.WriteSaveSlot(saveSlot, Encoding.UTF8.GetBytes(text2), delegate (bool didSave)
						{
							this.HideSaveIcon();
							if (callback != null)
							{
								callback(didSave);
							}
						});
					}
				}
				catch (Exception ex)
				{
					string str = "GM Save - There was an error saving the game: ";
					Exception ex2 = ex;
					Debug.LogError(str + ((ex2 != null) ? ex2.ToString() : null));
					this.HideSaveIcon();
					if (callback != null)
					{
						CoreLoop.InvokeNext(delegate
						{
							callback(false);
						});
					}
				}
				ModHooks.OnSavegameSave(saveSlot);
				return;
			}
			Debug.Log("Saving game disabled. No save file written.");
			if (callback != null)
			{
				CoreLoop.InvokeNext(delegate
				{
					callback(false);
				});
				return;
			}
		}
		else
		{
			Debug.LogError("Save game slot not valid: " + saveSlot.ToString());
			if (callback != null)
			{
				CoreLoop.InvokeNext(delegate
				{
					callback(false);
				});
			}
		}*/
	}

	// Token: 0x06001005 RID: 4101 RVA: 0x0004D868 File Offset: 0x0004BA68
	public void LoadGameFromUI(int saveSlot)
	{
		base.StartCoroutine(this.LoadGameFromUIRoutine(saveSlot));
	}

	// Token: 0x06001006 RID: 4102 RVA: 0x0004D878 File Offset: 0x0004BA78
	private IEnumerator LoadGameFromUIRoutine(int saveSlot)
	{
		//this.ui.ContinueGame();
		bool finishedLoading = false;
		bool successfullyLoaded = false;
		this.LoadGame(saveSlot, delegate (bool didLoad)
		{
			finishedLoading = true;
			successfullyLoaded = didLoad;
		});
		while (!finishedLoading)
		{
			yield return null;
		}
		if (successfullyLoaded)
		{
			this.ContinueGame();
		}
		else
		{
			//this.ui.UIGoToMainMenu();
		}
		yield break;
	}

	// Token: 0x06001007 RID: 4103 RVA: 0x0004D890 File Offset: 0x0004BA90
	public void LoadGame(int saveSlot, Action<bool> callback)
	{
		/*if (!Platform.IsSaveSlotIndexValid(saveSlot))
		{
			Debug.LogErrorFormat("Cannot load from invalid save slot index {0}", new object[]
			{
				saveSlot
			});
			if (callback != null)
			{
				CoreLoop.InvokeNext(delegate
				{
					callback(false);
				});
			}
			return;
		}
		try
		{
			string path = GameManager.ModdedSavePath(saveSlot);
			if (File.Exists(path))
			{
				using (FileStream fileStream = File.OpenRead(path))
				{
					using (StreamReader streamReader = new StreamReader(fileStream))
					{
						string text = streamReader.ReadToEnd();
						this.moddedData = JsonConvert.DeserializeObject<ModSavegameData>(text, new JsonSerializerSettings
						{
							ContractResolver = ShouldSerializeContractResolver.Instance,
							TypeNameHandling = TypeNameHandling.Auto,
							ObjectCreationHandling = ObjectCreationHandling.Replace,
							Converters = JsonConverterTypes.ConverterTypes
						});
						if (this.moddedData == null)
						{
							Modding.Logger.APILogger.LogError("Loaded mod savegame data deserialized to null: " + text);
							this.moddedData = new ModSavegameData();
						}
					}
					goto IL_110;
				}
			}
			this.moddedData = new ModSavegameData();
		IL_110:;
		}
		catch (Exception message)
		{
			Modding.Logger.APILogger.LogError(message);
			this.moddedData = new ModSavegameData();
		}
		ModHooks.OnLoadLocalSettings(this.moddedData);
		Platform.Current.ReadSaveSlot(saveSlot, delegate (byte[] fileBytes)
		{
			bool obj;
			try
			{
				string text2;
				if (this.gameConfig.useSaveEncryption && !Platform.Current.IsFileSystemProtected)
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					MemoryStream serializationStream = new MemoryStream(fileBytes);
					text2 = Encryption.Decrypt((string)binaryFormatter.Deserialize(serializationStream));
				}
				else
				{
					text2 = Encoding.UTF8.GetString(fileBytes);
				}
				SaveGameData saveGameData;
				try
				{
					saveGameData = JsonConvert.DeserializeObject<SaveGameData>(text2, new JsonSerializerSettings
					{
						ContractResolver = ShouldSerializeContractResolver.Instance,
						TypeNameHandling = TypeNameHandling.Auto,
						ObjectCreationHandling = ObjectCreationHandling.Replace,
						Converters = JsonConverterTypes.ConverterTypes
					});
				}
				catch (Exception message2)
				{
					Modding.Logger.APILogger.LogError("Failed to read save using Json.NET (GameManager::LoadGame), falling back.");
					Modding.Logger.APILogger.LogError(message2);
					saveGameData = JsonUtility.FromJson<SaveGameData>(text2);
				}
				PlayerData instance = saveGameData.playerData;
				SceneData instance2 = saveGameData.sceneData;
				PlayerData.instance = instance;
				this.playerData = instance;
				SceneData.instance = instance2;
				ModHooks.OnAfterSaveGameLoad(saveGameData);
				this.sceneData = instance2;
				this.profileID = saveSlot;
				this.inputHandler.RefreshPlayerData();
				ModHooks.OnSavegameLoad(saveSlot);
				obj = true;
			}
			catch (Exception ex)
			{
				Debug.LogFormat("Error loading save file for slot {0}: {1}", new object[]
				{
					saveSlot,
					ex
				});
				obj = false;
			}
			if (callback != null)
			{
				callback(obj);
			}
		});*/
	}

	// Token: 0x06001008 RID: 4104 RVA: 0x0004DA1C File Offset: 0x0004BC1C
	public void ClearSaveFile(int saveSlot, Action<bool> callback)
	{
		/*ModHooks.OnSavegameClear(saveSlot);
		this.orig_ClearSaveFile(saveSlot, callback);
		ModHooks.OnAfterSaveGameClear(saveSlot);*/
	}

	// Token: 0x06001009 RID: 4105 RVA: 0x0004DA34 File Offset: 0x0004BC34
	/*public void GetSaveStatsForSlot(int saveSlot, Action<SaveStats> callback)
	{
		if (!Platform.IsSaveSlotIndexValid(saveSlot))
		{
			Debug.LogErrorFormat("Cannot get save stats for invalid slot {0}", new object[]
			{
				saveSlot
			});
			if (callback != null)
			{
				CoreLoop.InvokeNext(delegate
				{
					callback(null);
				});
			}
			return;
		}
		Platform.Current.ReadSaveSlot(saveSlot, delegate (byte[] fileBytes)
		{
			if (fileBytes == null)
			{
				if (callback != null)
				{
					CoreLoop.InvokeNext(delegate
					{
						callback(null);
					});
				}
				return;
			}
			try
			{
				string text;
				if (this.gameConfig.useSaveEncryption && !Platform.Current.IsFileSystemProtected)
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					MemoryStream serializationStream = new MemoryStream(fileBytes);
					text = Encryption.Decrypt((string)binaryFormatter.Deserialize(serializationStream));
				}
				else
				{
					text = Encoding.UTF8.GetString(fileBytes);
				}
				SaveGameData saveGameData;
				try
				{
					saveGameData = JsonConvert.DeserializeObject<SaveGameData>(text, new JsonSerializerSettings
					{
						ContractResolver = ShouldSerializeContractResolver.Instance,
						TypeNameHandling = TypeNameHandling.Auto,
						ObjectCreationHandling = ObjectCreationHandling.Replace,
						Converters = JsonConverterTypes.ConverterTypes
					});
				}
				catch (Exception)
				{
					Modding.Logger.APILogger.LogWarn(string.Format("Failed to get save stats for slot {0} using Json.NET, falling back", saveSlot));
					saveGameData = JsonUtility.FromJson<SaveGameData>(text);
				}
				PlayerData playerData = saveGameData.playerData;
				SaveStats saveStats = new SaveStats(playerData.maxHealthBase, playerData.geo, playerData.mapZone, playerData.playTime, playerData.MPReserveMax, playerData.permadeathMode, playerData.bossRushMode, playerData.completionPercentage, playerData.unlockedCompletionRate);
				if (callback != null)
				{
					CoreLoop.InvokeNext(delegate
					{
						callback(saveStats);
					});
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Error while loading save file for slot ",
					saveSlot,
					" Exception: ",
					ex
				}));
				if (callback != null)
				{
					CoreLoop.InvokeNext(delegate
					{
						callback(null);
					});
				}
			}
		});
	}*/

	// Token: 0x0600100A RID: 4106 RVA: 0x0004DABD File Offset: 0x0004BCBD
	public IEnumerator PauseGameToggleByMenu()
	{
		yield return null;
		IEnumerator iterator = this.PauseGameToggle();
		while (iterator.MoveNext())
		{
			object obj = iterator.Current;
			yield return obj;
		}
		yield break;
	}

	// Token: 0x0600100B RID: 4107 RVA: 0x0004DACC File Offset: 0x0004BCCC
	public IEnumerator PauseGameToggle()
	{
		if (!this.TimeSlowed)
		{
			if (/*!this.playerData.disablePause && */this.gameState == GameState.PLAYING)
			{
				//this.gameCams.StopCameraShake();
				//this.inputHandler.PreventPause();
				//this.inputHandler.StopUIInput();
				//this.actorSnapshotPaused.TransitionTo(0f);
				this.isPaused = true;
				this.SetState(GameState.PAUSED);
				//this.ui.AudioGoToPauseMenu(0.2f);
				//this.ui.SetState(UIState.PAUSED);
				if (HeroController.instance != null)
				{
					HeroController.instance.Pause();
				}
				//this.gameCams.MoveMenuToHUDCamera();
				this.SetTimeScale(0f);
				yield return new WaitForSecondsRealtime(0.8f);
				//this.inputHandler.AllowPause();
			}
			else if (this.gameState == GameState.PAUSED)
			{
				//this.gameCams.ResumeCameraShake();
				//this.inputHandler.PreventPause();
				//this.actorSnapshotUnpaused.TransitionTo(0f);
				this.isPaused = false;
				//this.ui.AudioGoToGameplay(0.2f);
				//this.ui.SetState(UIState.PLAYING);
				this.SetState(GameState.PLAYING);
				if (HeroController.instance != null)
				{
					HeroController.instance.UnPause();
				}
				//MenuButtonList.ClearAllLastSelected();
				this.SetTimeScale(1f);
				yield return new WaitForSecondsRealtime(0.8f);
				//this.inputHandler.AllowPause();
			}
		}
		yield break;
	}

	// Token: 0x0600100C RID: 4108 RVA: 0x0004DADB File Offset: 0x0004BCDB
	private IEnumerator SetTimeScale(float newTimeScale, float duration)
	{
		float lastTimeScale = Time.timeScale;
		for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
		{
			float val = Mathf.Clamp01(timer / duration);
			SetTimeScale(Mathf.Lerp(lastTimeScale, newTimeScale, val));
			yield return null;
		}
		SetTimeScale(newTimeScale);
		yield break;
	}

	private void SetTimeScale(float newTimeScale)
	{
		if (timeSlowedCount > 1)
		{
			newTimeScale = Mathf.Min(newTimeScale, Time.timeScale);
		}
		Time.timeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale);
	}
	/*private IEnumerator SetTimeScale(float newTimeScale, float duration)
	{
		float lastTimeScale = TimeController.GenericTimeScale;
		for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
		{
			float t = Mathf.Clamp01(timer / duration);
			this.SetTimeScale(Mathf.Lerp(lastTimeScale, newTimeScale, t));
			yield return null;
		}
		this.SetTimeScale(newTimeScale);
		yield break;
	}

	// Token: 0x0600100D RID: 4109 RVA: 0x0004DAF8 File Offset: 0x0004BCF8
	private void SetTimeScale(float newTimeScale)
	{
		if (this.timeSlowedCount > 1)
		{
			newTimeScale = Mathf.Min(newTimeScale, TimeController.GenericTimeScale);
		}
		TimeController.GenericTimeScale = ((newTimeScale > 0.01f) ? newTimeScale : 0f);
	}*/

	// Token: 0x0600100E RID: 4110 RVA: 0x0004DB28 File Offset: 0x0004BD28
	public void FreezeMoment(int type)
	{
		if (type == 0)
		{
			base.StartCoroutine(this.FreezeMoment(0.01f, 0.35f, 0.1f, 0f));
		}
		else if (type == 1)
		{
			base.StartCoroutine(this.FreezeMoment(0.04f, 0.03f, 0.04f, 0f));
		}
		else if (type == 2)
		{
			base.StartCoroutine(this.FreezeMoment(0.25f, 2f, 0.25f, 0.15f));
		}
		else if (type == 3)
		{
			base.StartCoroutine(this.FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
		}
		if (type == 4)
		{
			base.StartCoroutine(this.FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
		}
		if (type == 5)
		{
			base.StartCoroutine(this.FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
		}
	}

	object settings = null;

	FieldInfo disableFreezeF = null;

	// Token: 0x0600100F RID: 4111 RVA: 0x0004DC1B File Offset: 0x0004BE1B
	public IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
	{
        if (settings == null)
        {
			var settingsType = FindType("WeaverCore.Editor", "WeaverCore.Editor.GeneralSettings");

			settings = settingsType.GetProperty("Instance").GetValue(null);

			disableFreezeF = settingsType.GetField("DisableGameFreezing");

		}

        if (((bool)disableFreezeF.GetValue(settings)) == true)
        {
			yield break;
        }

		this.timeSlowedCount++;
		yield return this.StartCoroutine(this.SetTimeScale(targetSpeed, rampDownTime));
		for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		yield return this.StartCoroutine(this.SetTimeScale(1f, rampUpTime));
		this.timeSlowedCount--;
		yield break;
	}

	// Token: 0x06001010 RID: 4112 RVA: 0x0004DC47 File Offset: 0x0004BE47
	public IEnumerator FreezeMomentGC(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
	{
		if (settings == null)
		{
			var settingsType = FindType("WeaverCore.Editor", "WeaverCore.Editor.GeneralSettings");

			settings = settingsType.GetProperty("Instance").GetValue(null);

			disableFreezeF = settingsType.GetField("DisableGameFreezing");

		}

		if (((bool)disableFreezeF.GetValue(settings)) == true)
		{
			yield break;
		}
		this.timeSlowedCount++;
		yield return this.StartCoroutine(this.SetTimeScale(targetSpeed, rampDownTime));
		for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		yield return this.StartCoroutine(this.SetTimeScale(1f, rampUpTime));
		this.timeSlowedCount--;
		yield break;
	}

	// Token: 0x06001011 RID: 4113 RVA: 0x0004DC73 File Offset: 0x0004BE73
	public IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, bool runGc = false)
	{
		this.timeSlowedCount++;
		yield return this.StartCoroutine(this.SetTimeScale(0f, rampDownTime));
		for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		yield return this.StartCoroutine(this.SetTimeScale(1f, rampUpTime));
		this.timeSlowedCount--;
		yield break;
	}

	// Token: 0x06001012 RID: 4114 RVA: 0x0004DC97 File Offset: 0x0004BE97
	public void EnsureSaveSlotSpace(Action<bool> callback)
	{
		//Platform.Current.EnsureSaveSlotSpace(this.profileID, callback);
	}

	// Token: 0x06001013 RID: 4115 RVA: 0x0004DCAC File Offset: 0x0004BEAC
	public void StartNewGame(bool permadeathMode = false, bool bossRushMode = false)
	{
		if (permadeathMode)
		{
			this.playerData.SetIntSwappedArgs(1, "permadeathMode");
		}
		else
		{
			this.playerData.SetIntSwappedArgs(0, "permadeathMode");
		}
		this.MatchBackerCreditsSetting();
		if (bossRushMode)
		{
			//this.playerData.AddGGPlayerDataOverrides();
			base.StartCoroutine(this.RunContinueGame());
			return;
		}
		base.StartCoroutine(this.RunStartNewGame());
	}

	// Token: 0x06001014 RID: 4116 RVA: 0x0004DD0F File Offset: 0x0004BF0F
	public IEnumerator RunStartNewGame()
	{
		this.cameraCtrl.FadeOut(CameraFadeType.START_FADE);
		//this.noMusicSnapshot.TransitionTo(2f);
		//this.noAtmosSnapshot.TransitionTo(2f);
		yield return new WaitForSeconds(2.6f);
		//this.ui.MakeMenuLean();
		this.BeginSceneTransition(new GameManager.SceneLoadInfo
		{
			AlwaysUnloadUnusedAssets = true,
			IsFirstLevelForPlayer = true,
			PreventCameraFadeOut = true,
			WaitForSceneTransitionCameraFade = false,
			SceneName = "Opening_Sequence",
			Visualization = GameManager.SceneLoadVisualizations.Custom
		});
		yield break;
	}

	// Token: 0x06001015 RID: 4117 RVA: 0x0004DD1E File Offset: 0x0004BF1E
	public void ContinueGame()
	{
		this.MatchBackerCreditsSetting();
		base.StartCoroutine(this.RunContinueGame());
	}

	// Token: 0x06001016 RID: 4118 RVA: 0x0004DD33 File Offset: 0x0004BF33
	public IEnumerator RunContinueGame()
	{
		this.cameraCtrl.FadeOut(CameraFadeType.START_FADE);
		//this.noMusicSnapshot.TransitionTo(2f);
		//this.noAtmosSnapshot.TransitionTo(2f);
		yield return new WaitForSeconds(2.6f);
		//this.audioManager.ApplyMusicCue(this.noMusicCue, 0f, 0f, false);
		//this.ui.MakeMenuLean();
		this.isLoading = true;
		this.SetState(GameState.LOADING);
		this.loadVisualization = GameManager.SceneLoadVisualizations.Default;
		//SaveDataUpgradeHandler.UpgradeSaveData(ref this.playerData);
		yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Knight_Pickup", LoadSceneMode.Additive);
		this.SetupSceneRefs(false);
		yield return null;
		UnityEngine.SceneManagement.SceneManager.UnloadScene("Knight_Pickup");
		this.needFirstFadeIn = true;
		this.isLoading = false;
		this.ReadyForRespawn(true);
		yield break;
	}

	// Token: 0x06001017 RID: 4119 RVA: 0x0004DD42 File Offset: 0x0004BF42
	public IEnumerator ReturnToMainMenu(GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback = null)
	{
		/*if (BossSequenceController.IsInSequence)
		{
			BossSequenceController.RestoreBindings();
		}*/
		StoryRecord_quit();
		TimePasses();
		if (saveMode != ReturnToMainMenuSaveModes.DontSave)
		{
			bool? saveComplete = null;
			SaveGame(profileID, delegate (bool didSave)
			{
				saveComplete = didSave;
			});
			while (!saveComplete.HasValue)
			{
				yield return null;
			}
			callback?.Invoke(saveComplete.Value);
			if (saveMode == ReturnToMainMenuSaveModes.SaveAndCancelOnFail && !saveComplete.Value)
			{
				yield break;
			}
		}
		else
		{
			callback?.Invoke(obj: false);
		}
		cameraCtrl.FreezeInPlace(freezeTargetAlso: true);
		cameraCtrl.FadeOut(CameraFadeType.JUST_FADE);
		//noMusicSnapshot.TransitionTo(1.5f);
		//noAtmosSnapshot.TransitionTo(1.5f);
		for (float timer = 0f; timer < 2f; timer += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		//StandaloneLoadingSpinner standaloneLoadingSpinner = UnityEngine.Object.Instantiate(standaloneLoadingSpinnerPrefab);
		//standaloneLoadingSpinner.Setup(this);
		//UnityEngine.Object.DontDestroyOnLoad(standaloneLoadingSpinner.gameObject);
		if (this.UnloadingLevel != null)
		{
			try
			{
				this.UnloadingLevel();
			}
			catch (Exception exception)
			{
				Debug.LogErrorFormat("Error while UnloadingLevel in QuitToMenu, attempting to continue regardless.");
				Debug.LogException(exception);
			}
		}
		if (this.DestroyPersonalPools != null)
		{
			try
			{
				this.DestroyPersonalPools();
			}
			catch (Exception exception2)
			{
				Debug.LogErrorFormat("Error while DestroyingPersonalPools in QuitToMenu, attempting to continue regardless.");
				Debug.LogException(exception2);
			}
		}
		//PlayMakerFSM.BroadcastEvent("QUIT TO MENU");
		waitForManualLevelStart = true;
		//StaticVariableList.Clear();
		yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Quit_To_Menu", LoadSceneMode.Single);
	}

	// Token: 0x06001018 RID: 4120 RVA: 0x0004DD5F File Offset: 0x0004BF5F
	public void WillTerminateInBackground()
	{
		if (this.gameState == GameState.PLAYING || this.gameState == GameState.PAUSED)
		{
			Debug.LogFormat("Saving in background, because we're about to terminate.", Array.Empty<object>());
			this.SaveGame();
		}
	}

	// Token: 0x06001019 RID: 4121 RVA: 0x0004DD88 File Offset: 0x0004BF88
	/*void Platform.IDisengageHandler.OnDisengage(Action next)
	{
		if (this.gameState == GameState.PLAYING || this.gameState == GameState.PAUSED)
		{
			this.EmergencyReturnToMenu(delegate
			{
				next();
			});
			return;
		}
		next();
	}*/

	// Token: 0x0600101A RID: 4122 RVA: 0x0004DDD4 File Offset: 0x0004BFD4
	public GameManager.ControllerConnectionStates GetControllerConnectionState()
	{
		return GameManager.ControllerConnectionStates.PossiblyConnected;
		/*if (this.inputHandler == null)
		{
			return GameManager.ControllerConnectionStates.PossiblyConnected;
		}
		InputDevice activeDevice = InputManager.ActiveDevice;
		if (activeDevice == null)
		{
			return GameManager.ControllerConnectionStates.NullDevice;
		}
		if (activeDevice == InputDevice.Null)
		{
			return GameManager.ControllerConnectionStates.DummyDevice;
		}
		if (!activeDevice.IsAttached)
		{
			return GameManager.ControllerConnectionStates.DetachedDevice;
		}
		return GameManager.ControllerConnectionStates.ConnectedAndReady;*/
	}

	// Token: 0x0600101B RID: 4123 RVA: 0x0004DE14 File Offset: 0x0004C014
	/*private void EmergencyReturnToMenu(Action callback)
	{
		this.isEmergencyReturningToMenu = true;
		Debug.LogFormat("Performing emergency return to menu...", Array.Empty<object>());
		//if (BossSequenceController.IsInSequence)
		//{
		//	BossSequenceController.RestoreBindings();
		//}
		if (callback != null)
		{
			CoreLoop.InvokeNext(callback);
		}
		this.inputHandler.StopUIInput();
		this.cameraCtrl.FreezeInPlace(true);
		this.noMusicSnapshot.TransitionTo(0f);
		this.noAtmosSnapshot.TransitionTo(0f);
		StandaloneLoadingSpinner standaloneLoadingSpinner = UnityEngine.Object.Instantiate<StandaloneLoadingSpinner>(this.standaloneLoadingSpinnerPrefab);
		standaloneLoadingSpinner.Setup(this);
		UnityEngine.Object.DontDestroyOnLoad(standaloneLoadingSpinner.gameObject);
		if (this.UnloadingLevel != null)
		{
			try
			{
				this.UnloadingLevel();
			}
			catch (Exception exception)
			{
				Debug.LogErrorFormat("Error while UnloadingLevel in QuitToMenu, attempting to continue regardless.", Array.Empty<object>());
				Debug.LogException(exception);
			}
		}
		if (this.DestroyPersonalPools != null)
		{
			try
			{
				this.DestroyPersonalPools();
			}
			catch (Exception exception2)
			{
				Debug.LogErrorFormat("Error while DestroyingPersonalPools in QuitToMenu, attempting to continue regardless.", Array.Empty<object>());
				Debug.LogException(exception2);
			}
		}
		PlayMakerFSM.BroadcastEvent("QUIT TO MENU");
		this.waitForManualLevelStart = true;
		UnityEngine.SceneManagement.SceneManager.LoadScene("Quit_To_Menu", LoadSceneMode.Single);
	}*/

	// Token: 0x0600101C RID: 4124 RVA: 0x0004DF30 File Offset: 0x0004C130
	public IEnumerator QuitGame()
	{
		this.StoryRecord_quit();
		//FSMUtility.SendEventToGameObject(GameObject.Find("Quit Blanker"), "START FADE", false);
		yield return new WaitForSeconds(0.5f);
		Application.Quit();
		yield break;
	}

	// Token: 0x0600101D RID: 4125 RVA: 0x0004DF3F File Offset: 0x0004C13F
	public void LoadedBoss()
	{
		if (this.OnLoadedBoss != null)
		{
			this.OnLoadedBoss();
		}
	}

	// Token: 0x0600101E RID: 4126 RVA: 0x0004DF54 File Offset: 0x0004C154
	public void DoDestroyPersonalPools()
	{
		if (this.DestroyPersonalPools != null)
		{
			this.DestroyPersonalPools();
		}
	}

	// Token: 0x0600101F RID: 4127 RVA: 0x0004DF69 File Offset: 0x0004C169
	public float GetImplicitCinematicVolume()
	{
		//return Mathf.Clamp01(this.gameSettings.masterVolume / 10f) * Mathf.Clamp01(this.gameSettings.soundVolume / 10f);
		return 1f;
	}

	// Token: 0x06001022 RID: 4130 RVA: 0x0004DFCC File Offset: 0x0004C1CC
	/*[CompilerGenerated]
	public UIManager orig_get_ui()
	{
		return this.< ui > k__BackingField;
	}

	// Token: 0x06001023 RID: 4131 RVA: 0x0004DFD4 File Offset: 0x0004C1D4
	[CompilerGenerated]
	private void orig_set_ui(UIManager value)
	{
		this.< ui > k__BackingField = value;
	}*/

	// Token: 0x06001024 RID: 4132 RVA: 0x0004DFDD File Offset: 0x0004C1DD
	/*private void orig_OnApplicationQuit()
	{
		if (this.startedLanguageDisabled)
		{
			this.gameConfig.hideLanguageOption = true;
		}
	}*/

	// Token: 0x06001025 RID: 4133 RVA: 0x0004DFF4 File Offset: 0x0004C1F4
	public void orig_LoadScene(string destScene)
	{
		this.tilemapDirty = true;
		this.startedOnThisScene = false;
		this.nextSceneName = destScene;
		if (this.DestroyPersonalPools != null)
		{
			this.DestroyPersonalPools();
		}
		if (this.UnloadingLevel != null)
		{
			this.UnloadingLevel();
		}
		UnityEngine.SceneManagement.SceneManager.LoadScene(destScene);
	}

	// Token: 0x06001026 RID: 4134 RVA: 0x0004E044 File Offset: 0x0004C244
	public void orig_BeginSceneTransition(GameManager.SceneLoadInfo info)
	{
		//this.inventoryFSM.SendEvent("INVENTORY CANCEL");
		if (info.IsFirstLevelForPlayer)
		{
			this.ResetGameTimer();
		}
		/*if (BossSequenceController.IsInSequence)
		{
			if (BossSequenceController.ForceAssetUnload)
			{
				info.AlwaysUnloadUnusedAssets = true;
				Debug.Log("Asset unload forced by next boss scene.");
			}
			else if (HeroController.instance.IsDreamReturning)
			{
				info.AlwaysUnloadUnusedAssets = true;
				Debug.Log("Asset unload force by death in boss sequence.");
			}
		}
		else if (BossStatueLoadManager.ShouldUnload)
		{
			info.AlwaysUnloadUnusedAssets = true;
			Debug.Log("Asset unload forced by challenging new boss statue");
		}*/
		base.StartCoroutine(this.BeginSceneTransitionRoutine(info));
	}

	// Token: 0x06001027 RID: 4135 RVA: 0x0004E0D8 File Offset: 0x0004C2D8
	/*public void orig_ClearSaveFile(int saveSlot, Action<bool> callback)
	{
		if (!Platform.IsSaveSlotIndexValid(saveSlot))
		{
			Debug.LogErrorFormat("Cannot clear invalid save slot index {0}", new object[]
			{
				saveSlot
			});
			if (callback != null)
			{
				CoreLoop.InvokeNext(delegate
				{
					callback(false);
				});
			}
			return;
		}
		Debug.LogFormat("Save file {0} {1}", new object[]
		{
			saveSlot,
			"clearing..."
		});
		Platform.Current.ClearSaveSlot(saveSlot, delegate (bool didClear)
		{
			Debug.LogFormat("Save file {0} {1}", new object[]
			{
				saveSlot,
				didClear ? "cleared" : "failed to clear"
			});
			if (didClear)
			{
				this.playerData.Reset();
				this.sceneData.Reset();
			}
			if (callback != null)
			{
				callback(didClear);
			}
		});
	}*/

	// Token: 0x06001028 RID: 4136 RVA: 0x0004E187 File Offset: 0x0004C387
	public IEnumerator orig_PlayerDead(float waitTime)
	{
		this.cameraCtrl.FreezeInPlace(true);
		this.NoLongerFirstGame();
		this.ResetSemiPersistentItems();
		bool finishedSaving = false;
		this.SaveGame(this.profileID, delegate (bool didSave)
		{
			finishedSaving = true;
		});
		yield return new WaitForSeconds(waitTime);
		this.cameraCtrl.FadeOut(CameraFadeType.HERO_DEATH);
		yield return new WaitForSeconds(0.8f);
		while (!finishedSaving)
		{
			yield return null;
		}
		this.ReadyForRespawn(false);
		/*else if (this.playerData.permadeathMode == 2)
		{
			this.LoadScene("PermaDeath");
		}*/
		yield break;
	}

	// Token: 0x06001029 RID: 4137 RVA: 0x0004E19D File Offset: 0x0004C39D
	private static string ModdedSavePath(int slot)
	{
		return Path.Combine(Application.persistentDataPath, string.Format("user{0}.modded.json", slot));
	}

	// Token: 0x0600102A RID: 4138 RVA: 0x0004E1BC File Offset: 0x0004C3BC
	public void orig_SetupSceneRefs(bool refreshTilemapInfo)
	{
		this.UpdateSceneName();
		/*if (this.ui == null)
		{
			this.ui = UIManager.instance;
		}*/

		this.sm = GameObject.FindObjectOfType<SceneManager>();
		if (sm == null)
		{
			var sceneManagerType = FindType("WeaverCore", "WeaverCore.Components.WeaverSceneManager");
			var sceneDimProp = sceneManagerType.GetProperty("SceneDimensions");

			var placeHolderObject = new GameObject("PLACEHOLDER SCENE MANAGER");

			var placeholderManager = placeHolderObject.AddComponent(sceneManagerType);

			sm = (SceneManager)placeholderManager;

			/*Rect dim = (Rect)sceneDimProp.GetValue(placeholderManager);
			SceneDimensions = dim;
			sceneWidth = dim.width;
			sceneHeight = dim.height;*/
			//Debug.Log("Scene Manager missing from scene " + this.sceneName);
		}
		//GameObject gameObject = GameObject.FindGameObjectWithTag("SceneManager");
		/*if (gameObject != null)
		{
			this.sm = gameObject.GetComponent<global::SceneManager>();
		}
		else
		{
			Debug.Log("Scene Manager missing from scene " + this.sceneName);
		}*/
		if (this.IsGameplayScene())
		{
			if (this.hero_ctrl == null)
			{
				this.SetupHeroRefs();
			}
			//this.inputHandler.AttachHeroController(this.hero_ctrl);
			if (refreshTilemapInfo)
			{
				this.RefreshTilemapInfo(this.sceneName);
			}
			//this.soulOrb_fsm = this.gameCams.soulOrbFSM;
			//this.soulVessel_fsm = this.gameCams.soulVesselFSM;
		}
	}

	// Token: 0x0600102B RID: 4139 RVA: 0x0004E27D File Offset: 0x0004C47D
	public void orig_OnWillActivateFirstLevel()
	{
		HeroController.instance.isEnteringFirstLevel = true;
		this.entryGateName = "top1";
		this.SetState(GameState.PLAYING);
		//this.ui.ConfigureMenu();
	}

	// Token: 0x0600102C RID: 4140 RVA: 0x0004E2A7 File Offset: 0x0004C4A7
	/*public IEnumerator PauseToggleDynamicMenu(MenuScreen screen, bool allowUnpause = false)
	{
		if (!this.TimeSlowed)
		{
			if (!this.playerData.disablePause && this.gameState == GameState.PLAYING)
			{
				this.gameCams.StopCameraShake();
				this.inputHandler.PreventPause();
				this.inputHandler.StopUIInput();
				this.actorSnapshotPaused.TransitionTo(0f);
				this.isPaused = true;
				this.SetState(GameState.PAUSED);
				this.ui.AudioGoToPauseMenu(0.2f);
				this.ui.UIPauseToDynamicMenu(screen);
				if (HeroController.instance != null)
				{
					HeroController.instance.Pause();
				}
				this.gameCams.MoveMenuToHUDCamera();
				this.SetTimeScale(0f);
				yield return new WaitForSecondsRealtime(0.8f);
				this.inputHandler.AllowPause();
			}
			else if (allowUnpause && this.gameState == GameState.PAUSED)
			{
				this.gameCams.ResumeCameraShake();
				this.inputHandler.PreventPause();
				this.actorSnapshotUnpaused.TransitionTo(0f);
				this.isPaused = false;
				this.ui.AudioGoToGameplay(0.2f);
				this.ui.SetState(UIState.PLAYING);
				this.SetState(GameState.PLAYING);
				if (HeroController.instance != null)
				{
					HeroController.instance.UnPause();
				}
				MenuButtonList.ClearAllLastSelected();
				this.SetTimeScale(1f);
				yield return new WaitForSecondsRealtime(0.8f);
				this.inputHandler.AllowPause();
			}
		}
		yield break;
	}*/

	[SerializeField]
	SupportedLanguages editorLanguage = SupportedLanguages.EN;

	// Token: 0x04000FE2 RID: 4066
	private bool verboseMode;

	// Token: 0x04000FE3 RID: 4067
	public GameState gameState;

	// Token: 0x04000FE4 RID: 4068
	public bool isPaused;

	// Token: 0x04000FE5 RID: 4069
	private int timeSlowedCount;

	// Token: 0x04000FE6 RID: 4070
	public string sceneName;

	// Token: 0x04000FE7 RID: 4071
	public string nextSceneName;

	// Token: 0x04000FE8 RID: 4072
	public string entryGateName;

	// Token: 0x04000FE9 RID: 4073
	private TransitionPoint callingGate;

	// Token: 0x04000FEA RID: 4074
	private Vector3 entrySpawnPoint;

	// Token: 0x04000FEB RID: 4075
	private float entryDelay;

	// Token: 0x04000FEC RID: 4076
	[HideInInspector]
	public float sceneWidth;

	// Token: 0x04000FED RID: 4077
	[HideInInspector]
	public float sceneHeight;

	// Token: 0x04000FEE RID: 4078
	//public GameConfig gameConfig;

	// Token: 0x04000FF0 RID: 4080
	private GameCameras gameCams;

	// Token: 0x04000FF2 RID: 4082
	//[SerializeField]
	//private AudioManager audioManager;

	// Token: 0x04000FF3 RID: 4083
	//[SerializeField]
	//private InControlManager inControlManagerPrefab;

	// Token: 0x04000FF4 RID: 4084
	//private static InControlManager _spawnedInControlManager;

	// Token: 0x04000FF5 RID: 4085
	//[SerializeField]
	//public GameSettings gameSettings;

	// Token: 0x04000FF6 RID: 4086
	//public TimeScaleIndependentUpdate timeTool;

	// Token: 0x04000FF7 RID: 4087
	[HideInInspector]
	public GameObject gameMap;

	// Token: 0x04001000 RID: 4096
	//public PlayMakerFSM inventoryFSM;

	// Token: 0x04001001 RID: 4097
	[SerializeField]
	public PlayerData playerData;

	// Token: 0x04001002 RID: 4098
	//[SerializeField]
	//public SceneData sceneData;

	// Token: 0x04001003 RID: 4099
	public const int NoSaveSlotID = -1;

	// Token: 0x04001004 RID: 4100
	[HideInInspector]
	public int profileID;

	// Token: 0x04001005 RID: 4101
	private bool needsFlush;

	// Token: 0x04001006 RID: 4102
	private bool isEmergencyReturningToMenu;

	// Token: 0x04001007 RID: 4103
	private float sessionPlayTimer;

	// Token: 0x04001008 RID: 4104
	private float sessionStartTime;

	// Token: 0x04001009 RID: 4105
	public bool startedOnThisScene = true;

	// Token: 0x0400100B RID: 4107
	private bool hazardRespawningHero;

	// Token: 0x0400100C RID: 4108
	private string targetScene;

	// Token: 0x0400100D RID: 4109
	private bool tilemapDirty;

	// Token: 0x0400100E RID: 4110
	private bool needFirstFadeIn;

	// Token: 0x0400100F RID: 4111
	private bool waitForManualLevelStart;

	// Token: 0x04001010 RID: 4112
	private bool startedSteamEnabled;

	// Token: 0x04001011 RID: 4113
	private bool startedGOGEnabled;

	// Token: 0x04001012 RID: 4114
	private bool startedLanguageDisabled;

	// Token: 0x04001013 RID: 4115
	//public AudioMixerSnapshot actorSnapshotUnpaused;

	// Token: 0x04001014 RID: 4116
	//public AudioMixerSnapshot actorSnapshotPaused;

	// Token: 0x04001015 RID: 4117
	//public AudioMixerSnapshot silentSnapshot;

	// Token: 0x04001016 RID: 4118
	//public AudioMixerSnapshot noMusicSnapshot;

	// Token: 0x04001017 RID: 4119
	//public MusicCue noMusicCue;

	// Token: 0x04001018 RID: 4120
	//public AudioMixerSnapshot noAtmosSnapshot;

	// Token: 0x04001023 RID: 4131
	private bool hasFinishedEnteringScene;

	// Token: 0x04001024 RID: 4132
	//[SerializeField]
	//private WorldInfo worldInfo;

	// Token: 0x04001025 RID: 4133
	private bool isLoading;

	// Token: 0x04001026 RID: 4134
	private GameManager.SceneLoadVisualizations loadVisualization;

	// Token: 0x04001027 RID: 4135
	private float currentLoadDuration;

	// Token: 0x04001028 RID: 4136
	private int sceneLoadsWithoutGarbageCollect;

	// Token: 0x04001029 RID: 4137
	private bool isUsingCustomLoadAnimation;

	// Token: 0x0400102A RID: 4138
	//[SerializeField]
	//private StandaloneLoadingSpinner standaloneLoadingSpinnerPrefab;

	// Token: 0x0400102B RID: 4139
	public static GameManager _instance;

	// Token: 0x0400102C RID: 4140
	private SceneLoad sceneLoad;

	// Token: 0x0400102E RID: 4142
	private static readonly string[] SubSceneNameSuffixes = new string[]
	{
		"_boss_defeated",
		"_boss",
		"_preload"
	};

	// Token: 0x0400102F RID: 4143
	private int saveIconShowCounter;

	// Token: 0x04001030 RID: 4144
	//private ModSavegameData moddedData;

	// Token: 0x04001031 RID: 4145
	//private UIManager _uiInstance;

	// Token: 0x020002D5 RID: 725
	// (Invoke) Token: 0x0600102E RID: 4142
	public delegate void SavePersistentState();

	// Token: 0x020002D6 RID: 726
	// (Invoke) Token: 0x06001032 RID: 4146
	public delegate void ResetSemiPersistentState();

	// Token: 0x020002D7 RID: 727
	// (Invoke) Token: 0x06001036 RID: 4150
	public delegate void DestroyPooledObjects();

	// Token: 0x020002D8 RID: 728
	// (Invoke) Token: 0x0600103A RID: 4154
	public delegate void UnloadLevel();

	// Token: 0x020002D9 RID: 729
	// (Invoke) Token: 0x0600103E RID: 4158
	public delegate void RefreshLanguage();

	// Token: 0x020002DA RID: 730
	// (Invoke) Token: 0x06001042 RID: 4162
	public delegate void RefreshParticles();

	// Token: 0x020002DB RID: 731
	// (Invoke) Token: 0x06001046 RID: 4166
	public delegate void BossLoad();

	// Token: 0x020002DC RID: 732
	// (Invoke) Token: 0x0600104A RID: 4170
	public delegate void EnterSceneEvent();

	// Token: 0x020002DD RID: 733
	// (Invoke) Token: 0x0600104E RID: 4174
	public delegate void SceneTransitionFinishEvent();

	// Token: 0x020002DE RID: 734
	public enum SceneLoadVisualizations
	{
		// Token: 0x04001033 RID: 4147
		Default,
		// Token: 0x04001034 RID: 4148
		Custom = -1,
		// Token: 0x04001035 RID: 4149
		Dream = 1,
		// Token: 0x04001036 RID: 4150
		Colosseum,
		// Token: 0x04001037 RID: 4151
		GrimmDream,
		// Token: 0x04001038 RID: 4152
		ContinueFromSave,
		// Token: 0x04001039 RID: 4153
		GodsAndGlory
	}

	// Token: 0x020002DF RID: 735
	public class SceneLoadInfo
	{
		// Token: 0x06001051 RID: 4177 RVA: 0x00003603 File Offset: 0x00001803
		public virtual void NotifyFetchComplete()
		{
		}

		// Token: 0x06001052 RID: 4178 RVA: 0x0004E2C4 File Offset: 0x0004C4C4
		public virtual bool IsReadyToActivate()
		{
			return true;
		}

		// Token: 0x06001053 RID: 4179 RVA: 0x00003603 File Offset: 0x00001803
		public virtual void NotifyFinished()
		{
		}

		// Token: 0x0400103A RID: 4154
		public bool IsFirstLevelForPlayer;

		// Token: 0x0400103B RID: 4155
		public string SceneName;

		// Token: 0x0400103C RID: 4156
		public GatePosition? HeroLeaveDirection;

		// Token: 0x0400103D RID: 4157
		public string EntryGateName;

		// Token: 0x0400103E RID: 4158
		public float EntryDelay;

		// Token: 0x0400103F RID: 4159
		public bool PreventCameraFadeOut;

		// Token: 0x04001040 RID: 4160
		public bool WaitForSceneTransitionCameraFade;

		// Token: 0x04001041 RID: 4161
		public GameManager.SceneLoadVisualizations Visualization;

		// Token: 0x04001042 RID: 4162
		public bool AlwaysUnloadUnusedAssets;

		// Token: 0x04001043 RID: 4163
		public bool forceWaitFetch;
	}

	// Token: 0x020002E0 RID: 736
	// (Invoke) Token: 0x06001056 RID: 4182
	public delegate void SceneTransitionBeganDelegate(SceneLoad sceneLoad);

	// Token: 0x020002E1 RID: 737
	public enum ReturnToMainMenuSaveModes
	{
		// Token: 0x04001045 RID: 4165
		SaveAndCancelOnFail,
		// Token: 0x04001046 RID: 4166
		SaveAndContinueOnFail,
		// Token: 0x04001047 RID: 4167
		DontSave
	}

	// Token: 0x020002E2 RID: 738
	public enum ControllerConnectionStates
	{
		// Token: 0x04001049 RID: 4169
		DetachedDevice,
		// Token: 0x0400104A RID: 4170
		DummyDevice,
		// Token: 0x0400104B RID: 4171
		NullDevice,
		// Token: 0x0400104C RID: 4172
		PossiblyConnected,
		// Token: 0x0400104D RID: 4173
		ConnectedAndReady
	}
}


/*using GlobalEnums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public delegate void UnloadLevel();
	public event UnloadLevel UnloadingLevel;

	private static readonly string[] SubSceneNameSuffixes = new string[]
	{
		"_boss_defeated",
		"_boss",
		"_preload"
	};

	static GameManager dummyInstance;

	public bool isPaused => false;
	public HeroController hero_ctrl => HeroController.instance;
	public CameraController cameraCtrl => null;//TODO TODO TODO TODO
	public bool RespawningHero { get; set; }
	public bool IsInSceneTransition { get; private set; }

	private int timeSlowedCount;
	private bool hazardRespawningHero;
	//private GameCameras gameCams;
	public GameState gameState = GameState.PLAYING;
	public float sceneWidth;
	public float sceneHeight;
	public string sceneName;
	public string entryGateName;
	private bool verboseMode = false;
	public string nextSceneName;
	public bool startedOnThisScene = true;


	static SceneManager dummySceneManager;
	public global::SceneManager sm
	{
		get
		{
			if (dummySceneManager == null)
			{
				dummySceneManager = GameManager.instance.gameObject.AddComponent<SceneManager>();
			}
			return dummySceneManager;
		}
		private set
		{
			dummySceneManager = value;
		}
	}


	public static GameManager instance
	{
		get
		{
			if (dummyInstance == null)
			{
				dummyInstance = new GameObject("GAME_MANAGER").AddComponent<GameManager>();
				DontDestroyOnLoad(dummyInstance.gameObject);
			}
			return dummyInstance;
		}
	}

	public IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
	{
		timeSlowedCount++;
		yield return StartCoroutine(SetTimeScale(targetSpeed, rampDownTime));
		for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		yield return StartCoroutine(SetTimeScale(1f, rampUpTime));
		timeSlowedCount--;
		yield break;
	}

	private IEnumerator SetTimeScale(float newTimeScale, float duration)
	{
		float lastTimeScale = Time.timeScale;
		for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
		{
			float val = Mathf.Clamp01(timer / duration);
			SetTimeScale(Mathf.Lerp(lastTimeScale, newTimeScale, val));
			yield return null;
		}
		SetTimeScale(newTimeScale);
		yield break;
	}

	private void SetTimeScale(float newTimeScale)
	{
		if (timeSlowedCount > 1)
		{
			newTimeScale = Mathf.Min(newTimeScale, Time.timeScale);
		}
		Time.timeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale);
	}

	public void AwardAchievement(string key)
	{

	}

	public void BeginSceneTransition(GameManager.SceneLoadInfo info)
	{
		info.SceneName = ModHooks.BeforeSceneLoad(info.SceneName);
		this.orig_BeginSceneTransition(info);
	}

	public void orig_BeginSceneTransition(GameManager.SceneLoadInfo info)
	{
		//this.inventoryFSM.SendEvent("INVENTORY CANCEL");
		if (info.IsFirstLevelForPlayer)
		{
			this.ResetGameTimer();
		}
		if (BossSequenceController.IsInSequence)
		{
			if (BossSequenceController.ForceAssetUnload)
			{
				info.AlwaysUnloadUnusedAssets = true;
				Debug.Log("Asset unload forced by next boss scene.");
			}
			else if (HeroController.instance.IsDreamReturning)
			{
				info.AlwaysUnloadUnusedAssets = true;
				Debug.Log("Asset unload force by death in boss sequence.");
			}
		}
		else if (BossStatueLoadManager.ShouldUnload)
		{
			info.AlwaysUnloadUnusedAssets = true;
			Debug.Log("Asset unload forced by challenging new boss statue");
		}
		base.StartCoroutine(this.BeginSceneTransitionRoutine(info));
	}

	private void Awake()
	{
		//hero_ctrl = HeroController.instance;
		//gameCams = GameCameras.instance;
	}

	public void BeginScene()
	{
		//this.inputHandler.SceneInit();
		//this.ui.SceneInit();
		if (this.hero_ctrl)
		{
			this.hero_ctrl.SceneInit();
		}
		GameCameras.instance.SceneInit();
		if (this.IsMenuScene())
		{
			this.SetState(GameState.MAIN_MENU);
			//this.UpdateUIStateFromGameState();
			//Platform.Current.SetSocialPresence("IN_MENU", true);
			return;
		}
		if (this.IsGameplayScene())
		{
			if ((!Application.isEditor && !Debug.isDebugBuild) || Time.renderedFrameCount > 3)
			{
				this.PositionHeroAtSceneEntrance();
			}
			//if (this.sm != null)
			//{
				//Platform.Current.SetSocialPresence("EXPLORING_" + this.sm.mapZone.ToString(), true);
			//	return;
			//}
		}
		else
		{
			if (this.IsNonGameplayScene())
			{
				this.SetState(GameState.CUTSCENE);
				//this.UpdateUIStateFromGameState();
				return;
			}
			Debug.LogError("GM - Scene type is not set to a standard scene type.");
			//this.UpdateUIStateFromGameState();
		}
	}

	private void EnterHero(bool additiveGateSearch = false)
	{
		if (this.entryGateName == "door_dreamReturn" && !string.IsNullOrEmpty(PlayerData.instance.GetString("bossReturnEntryGate")))
		{
			if (this.GetCurrentMapZone() == MapZone.GODS_GLORY.ToString())
			{
				this.entryGateName = PlayerData.instance.GetString("bossReturnEntryGate");
			}
			PlayerData.instance.SetStringSwappedArgs(string.Empty, "bossReturnEntryGate");
		}
		if (this.RespawningHero)
		{
			//if (this.needFirstFadeIn)
			//{
			//	base.StartCoroutine(this.FadeSceneInWithDelay(0.3f));
			//	this.needFirstFadeIn = false;
			//}
			base.StartCoroutine(this.hero_ctrl.Respawn());
			this.FinishedEnteringScene();
			this.RespawningHero = false;
			return;
		}
		if (this.hazardRespawningHero)
		{
			base.StartCoroutine(this.hero_ctrl.HazardRespawn());
			this.FinishedEnteringScene();
			this.hazardRespawningHero = false;
			return;
		}
		if (this.entryGateName == "dreamGate")
		{
			this.hero_ctrl.EnterSceneDreamGate();
			return;
		}
		if (this.startedOnThisScene)
		{
			if (this.IsGameplayScene())
			{
				this.FinishedEnteringScene();
				//this.FadeSceneIn();
			}
			return;
		}
		this.SetState(GameState.ENTERING_LEVEL);
		if (string.IsNullOrEmpty(this.entryGateName))
		{
			Debug.LogError("No entry gate has been defined in the Game Manager, unable to move hero into position.");
			this.FinishedEnteringScene();
			return;
		}
		if (additiveGateSearch)
		{
			if (this.verboseMode)
			{
				Debug.Log("Searching for entry gate " + this.entryGateName + " in the next scene: " + this.nextSceneName);
			}
			foreach (GameObject gameObject in UnityEngine.SceneManagement.SceneManager.GetSceneByName(this.nextSceneName).GetRootGameObjects())
			{
				TransitionPoint component = gameObject.GetComponent<TransitionPoint>();
				if (component != null && component.name == this.entryGateName)
				{
					if (this.verboseMode)
					{
						Debug.Log("SUCCESS - Found as root object");
					}
					base.StartCoroutine(this.hero_ctrl.EnterScene(component, this.entryDelay));
					return;
				}
				if (gameObject.name == "_Transition Gates")
				{
					TransitionPoint[] componentsInChildren = gameObject.GetComponentsInChildren<TransitionPoint>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						if (componentsInChildren[j].name == this.entryGateName)
						{
							if (this.verboseMode)
							{
								Debug.Log("SUCCESS - Found in _Transition Gates folder");
							}
							base.StartCoroutine(this.hero_ctrl.EnterScene(componentsInChildren[j], this.entryDelay));
							return;
						}
					}
				}
				TransitionPoint[] componentsInChildren2 = gameObject.GetComponentsInChildren<TransitionPoint>();
				for (int k = 0; k < componentsInChildren2.Length; k++)
				{
					if (componentsInChildren2[k].name == this.entryGateName)
					{
						if (this.verboseMode)
						{
							Debug.Log("SUCCESS - Found as a child of a random scene object, can't win em all");
						}
						base.StartCoroutine(this.hero_ctrl.EnterScene(componentsInChildren2[k], this.entryDelay));
						return;
					}
				}
			}
			Debug.LogError("Searching in next scene for TransitionGate failed.");
			return;
		}
		GameObject gameObject2 = GameObject.Find(this.entryGateName);
		if (gameObject2 != null)
		{
			TransitionPoint component2 = gameObject2.GetComponent<TransitionPoint>();
			base.StartCoroutine(this.hero_ctrl.EnterScene(component2, this.entryDelay));
			return;
		}
		Debug.LogError(string.Concat(new string[]
		{
			"No entry point found with the name \"",
			this.entryGateName,
			"\" in this scene (",
			this.sceneName,
			"). Unable to move hero into position, trying alternative gates..."
		}));
		TransitionPoint[] array = UnityEngine.Object.FindObjectsOfType<TransitionPoint>();
		if (array != null)
		{
			base.StartCoroutine(this.hero_ctrl.EnterScene(array[0], this.entryDelay));
			return;
		}
		Debug.LogError("Could not find any gates in this scene. Trying last ditch spawn...");
		this.hero_ctrl.transform.SetPosition2D((float)this.tilemap.width / 2f, (float)this.tilemap.height / 2f);
		this.hero_ctrl.EnterSceneDreamGate();
	}

	private void PositionHeroAtSceneEntrance()
	{
		Vector2 position = this.FindEntryPoint(this.entryGateName, default(Scene)) ?? new Vector2(-20000f, 20000f);
		if (this.hero_ctrl != null)
		{
			//this.hero_ctrl.transform.SetPosition2D(position);
			hero_ctrl.transform.position = new Vector3(position.x,position.y,hero_ctrl.transform.position.z);
		}
	}

	private Vector2? FindEntryPoint(string entryPointName, Scene filterScene)
	{
		if (this.RespawningHero)
		{
			Transform transform = this.hero_ctrl.LocateSpawnPoint();
			if (transform != null)
			{
				return new Vector2?(transform.transform.position);
			}
			return null;
		}
		else
		{
			if (this.hazardRespawningHero)
			{
				return new Vector2?(PlayerData.instance.GetVector3("hazardRespawnLocation"));
			}
			if (this.entryGateName == "dreamGate")
			{
				return new Vector2?(new Vector2(PlayerData.instance.GetFloat("dreamGateX"), PlayerData.instance.GetFloat("dreamGateY")));
			}
			TransitionPoint transitionPoint = this.FindTransitionPoint(entryPointName, filterScene, true);
			if (transitionPoint != null)
			{
				return new Vector2?(transitionPoint.transform.position + transitionPoint.entryOffset);
			}
			return null;
		}
	}

	public void ReadyForRespawn(bool isFirstLevelForPlayer)
	{
		this.RespawningHero = true;
		this.BeginSceneTransition(new GameManager.SceneLoadInfo
		{
			PreventCameraFadeOut = true,
			WaitForSceneTransitionCameraFade = false,
			EntryGateName = "",
			SceneName = PlayerData.instance.GetString("respawnScene"),
			Visualization = (isFirstLevelForPlayer ? GameManager.SceneLoadVisualizations.ContinueFromSave : GameManager.SceneLoadVisualizations.Default),
			AlwaysUnloadUnusedAssets = true,
			IsFirstLevelForPlayer = isFirstLevelForPlayer
		});
	}

	// Token: 0x06000F70 RID: 3952 RVA: 0x0004B1B5 File Offset: 0x000493B5
	public void HazardRespawn()
	{
		this.hazardRespawningHero = true;
		this.entryGateName = "";
		this.cameraCtrl.ResetStartTimer();
		this.cameraCtrl.camTarget.mode = CameraTarget.TargetMode.FOLLOW_HERO;
		this.EnterHero(false);
	}

	public void SetCurrentMapZoneAsRespawn()
	{
		PlayerData.instance.SetVariableSwappedArgs<MapZone>(this.sm.mapZone, "mapZone");
	}

	// Token: 0x06000F8D RID: 3981 RVA: 0x0004B6AC File Offset: 0x000498AC
	public void SetMapZoneToSpecific(string mapZone)
	{
		object obj = Enum.Parse(typeof(MapZone), mapZone);
		if (obj != null)
		{
			PlayerData.instance.SetVariableSwappedArgs<MapZone>((MapZone)obj, "mapZone");
			return;
		}
		Debug.LogError("Couldn't convert " + mapZone + " to a MapZone");
	}

	public void SetState(GameState newState)
	{
		this.gameState = newState;
	}

	public static string GetBaseSceneName(string fullSceneName)
	{
		for (int i = 0; i < GameManager.SubSceneNameSuffixes.Length; i++)
		{
			string text = GameManager.SubSceneNameSuffixes[i];
			if (fullSceneName.EndsWith(text, StringComparison.InvariantCultureIgnoreCase))
			{
				return fullSceneName.Substring(0, fullSceneName.Length - text.Length);
			}
		}
		return fullSceneName;
	}

	public string GetCurrentMapZone()
	{
		return this.sm.mapZone.ToString();
	}

	public void UpdateSceneName()
	{
		this.sceneName = GameManager.GetBaseSceneName(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
	}

	public bool IsTitleScreenScene()
	{
		this.UpdateSceneName();
		return string.Compare(this.sceneName, "Title_Screens", true) == 0;
	}

	public bool IsGameplayScene()
	{
		this.UpdateSceneName();
		return !this.IsNonGameplayScene();
	}

	public bool IsNonGameplayScene()
	{
		this.UpdateSceneName();
		return this.IsCinematicScene() || this.sceneName == "Knight Pickup" || this.sceneName == "Pre_Menu_Intro" || this.sceneName == "Menu_Title" || this.sceneName == "End_Credits" || this.sceneName == "Menu_Credits" || this.sceneName == "Cutscene_Boss_Door" || this.sceneName == "PermaDeath_Unlock" || this.sceneName == "GG_Unlock" || this.sceneName == "GG_End_Sequence" || this.sceneName == "End_Game_Completion" || this.sceneName == "BetaEnd" || this.sceneName == "PermaDeath" || this.sceneName == "GG_Entrance_Cutscene" || this.sceneName == "GG_Boss_Door_Entrance" || InGameCutsceneInfo.IsInCutscene;
	}

	public bool IsMenuScene()
	{
		this.UpdateSceneName();
		return this.sceneName == "Menu_Title";
	}

	// Token: 0x06000FF3 RID: 4083 RVA: 0x0004D020 File Offset: 0x0004B220
	public bool IsCinematicScene()
	{
		this.UpdateSceneName();
		return this.sceneName == "Intro_Cutscene_Prologue" || this.sceneName == "Opening_Sequence" || this.sceneName == "Prologue_Excerpt" || this.sceneName == "Intro_Cutscene" || this.sceneName == "Cinematic_Stag_travel" || this.sceneName == "PermaDeath" || this.sceneName == "Cinematic_Ending_A" || this.sceneName == "Cinematic_Ending_B" || this.sceneName == "Cinematic_Ending_C" || this.sceneName == "Cinematic_Ending_D" || this.sceneName == "Cinematic_Ending_E" || this.sceneName == "Cinematic_MrMushroom" || this.sceneName == "BetaEnd";
	}

	public string GetSceneNameString()
	{
		this.UpdateSceneName();
		return this.sceneName;
	}

	public void LoadScene(string destScene)
	{
		destScene = ModHooks.BeforeSceneLoad(destScene);
		this.orig_LoadScene(destScene);
		ModHooks.OnSceneChanged(destScene);
	}

	public void LeftScene(bool doAdditiveLoad = false)
	{
		UnityEngine.SceneManagement.SceneManager.GetSceneByName(this.targetScene);
		if (doAdditiveLoad)
		{
			base.StartCoroutine(this.LoadSceneAdditive(this.targetScene));
			return;
		}
		this.LoadScene(this.targetScene);
	}

	public void orig_LoadScene(string destScene)
	{
		this.tilemapDirty = true;
		this.startedOnThisScene = false;
		this.nextSceneName = destScene;
		if (this.DestroyPersonalPools != null)
		{
			this.DestroyPersonalPools();
		}
		if (this.UnloadingLevel != null)
		{
			this.UnloadingLevel();
		}
		UnityEngine.SceneManagement.SceneManager.LoadScene(destScene);
	}

	public IEnumerator PlayerDead(float waitTime)
	{
		ModHooks.OnBeforePlayerDead();
		yield return this.orig_PlayerDead(waitTime);
		ModHooks.OnAfterPlayerDead();
		yield break;
	}

	public IEnumerator orig_PlayerDead(float waitTime)
	{
		this.cameraCtrl.FreezeInPlace(true);
		this.NoLongerFirstGame();
		this.ResetSemiPersistentItems();
		bool finishedSaving = false;
		this.SaveGame(this.profileID, delegate (bool didSave)
		{
			finishedSaving = true;
		});
		yield return new WaitForSeconds(waitTime);
		this.cameraCtrl.FadeOut(CameraFadeType.HERO_DEATH);
		yield return new WaitForSeconds(0.8f);
		while (!finishedSaving)
		{
			yield return null;
		}
		if (PlayerData.instance.permadeathMode == 0)
		{
			this.ReadyForRespawn(false);
		}
		else if (PlayerData.instance.permadeathMode == 2)
		{
			this.LoadScene("PermaDeath");
		}
		yield break;
	}

	// Token: 0x06000F6E RID: 3950 RVA: 0x0004B135 File Offset: 0x00049335
	public IEnumerator PlayerDeadFromHazard(float waitTime)
	{
		this.cameraCtrl.FreezeInPlace(true);
		this.NoLongerFirstGame();
		this.SaveLevelState();
		yield return new WaitForSeconds(waitTime);
		//this.cameraCtrl.FadeOut(CameraFadeType.HERO_HAZARD_DEATH);
		yield return new WaitForSeconds(0.8f);
		PlayMakerFSM.BroadcastEvent("HAZARD RELOAD");
		this.HazardRespawn();
		yield break;
	}

	// Token: 0x06000FC9 RID: 4041 RVA: 0x0004C0BA File Offset: 0x0004A2BA
	public IEnumerator LoadSceneAdditive(string destScene)
	{
		Debug.Log("Loading " + destScene);
		destScene = ModHooks.BeforeSceneLoad(destScene);
		this.tilemapDirty = true;
		this.startedOnThisScene = false;
		this.nextSceneName = destScene;
		this.waitForManualLevelStart = true;
		if (this.DestroyPersonalPools != null)
		{
			this.DestroyPersonalPools();
		}
		if (this.UnloadingLevel != null)
		{
			this.UnloadingLevel();
		}
		string exitingScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(destScene, LoadSceneMode.Additive);
		asyncOperation.allowSceneActivation = true;
		yield return asyncOperation;
		yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(exitingScene);
		ModHooks.OnSceneChanged(destScene);
		this.RefreshTilemapInfo(destScene);
		if (this.IsUnloadAssetsRequired(exitingScene, destScene))
		{
			Debug.LogFormat(this, "Unloading assets due to zone transition", new object[0]);
			yield return Resources.UnloadUnusedAssets();
		}
		this.SetupSceneRefs(true);
		this.BeginScene();
		this.OnNextLevelReady();
		this.waitForManualLevelStart = false;
		Debug.Log("Done Loading " + destScene);
		yield break;
	}

	private void SetupGameRefs()
	{
		PlayerData.instance = PlayerData.instance;
		this.sceneData = SceneData.instance;
		this.gameCams = GameCameras.instance;
		this.cameraCtrl = this.gameCams.cameraController;
		this.gameSettings = new GameSettings();
		this.inputHandler = base.GetComponent<InputHandler>();
		this.achievementHandler = base.GetComponent<AchievementHandler>();
		if (GameManager._spawnedInControlManager == null)
		{
			GameManager._spawnedInControlManager = UnityEngine.Object.Instantiate<InControlManager>(this.inControlManagerPrefab);
			UnityEngine.Object.DontDestroyOnLoad(GameManager._spawnedInControlManager);
		}
		this.inventoryFSM = this.gameCams.gameObject.transform.Find("HudCamera").gameObject.transform.Find("Inventory").gameObject.GetComponent<PlayMakerFSM>();
		if (AchievementPopupHandler.Instance)
		{
			AchievementPopupHandler.Instance.Setup(this.achievementHandler);
		}
		Platform.Current.AdjustGraphicsSettings(this.gameSettings);
		if (this.inputHandler == null)
		{
			Debug.LogError("Couldn't find InputHandler component.");
		}
		if (this.achievementHandler == null)
		{
			Debug.LogError("Couldn't find AchievementHandler component.");
		}
		UnityEngine.SceneManagement.SceneManager.activeSceneChanged += this.LevelActivated;
		//Platform.Current.SetDisengageHandler(this);
	}

	private void LevelActivated(Scene sceneFrom, Scene sceneTo)
	{
		if (this == GameManager._instance)
		{
			if (!this.waitForManualLevelStart)
			{
				Debug.LogFormat(this, "Performing automatic level start.", Array.Empty<object>());
				if (this.startedOnThisScene && this.IsGameplayScene())
				{
					this.tilemapDirty = true;
				}
				this.SetupSceneRefs(true);
				this.BeginScene();
				this.OnNextLevelReady();
				return;
			}
			Debug.LogFormat(this, "Deferring level start (marked as manual).", Array.Empty<object>());
		}
	}

	// Token: 0x06000FD6 RID: 4054 RVA: 0x0004C8A0 File Offset: 0x0004AAA0
	public void SetupSceneRefs(bool refreshTilemapInfo)
	{
		this.orig_SetupSceneRefs(refreshTilemapInfo);
		if (this.IsGameplayScene())
		{
			tk2dSpriteAnimator component = GameCameras.instance.soulOrbFSM.gameObject.transform.Find("SoulOrb_fill").gameObject.transform.Find("Liquid").gameObject.GetComponent<tk2dSpriteAnimator>();
			component.GetClipByName("Fill").fps = 15.749999f;
			component.GetClipByName("Idle").fps = 10.5f;
			component.GetClipByName("Shrink").fps = 15.749999f;
			component.GetClipByName("Drain").fps = 31.499998f;
		}
	}

	// Token: 0x06000FD7 RID: 4055 RVA: 0x0004C94E File Offset: 0x0004AB4E
	public void SetupHeroRefs()
	{
		this.hero_ctrl = HeroController.instance;
		this.heroLight = GameObject.FindGameObjectWithTag("HeroLightMain").GetComponent<SpriteRenderer>();
	}

	public void FinishedEnteringScene()
	{
		this.SetState(GameState.PLAYING);
		this.entryDelay = 0f;
		this.hasFinishedEnteringScene = true;
		if (this.OnFinishedEnteringScene != null)
		{
			this.OnFinishedEnteringScene();
		}
	}

	public void SetPlayerDataBool(string boolName, bool value)
	{
		PlayerData.instance.SetBool(boolName, value);
	}

	// Token: 0x06000F7A RID: 3962 RVA: 0x0004B54A File Offset: 0x0004974A
	public void SetPlayerDataInt(string intName, int value)
	{
		PlayerData.instance.SetInt(intName, value);
	}

	// Token: 0x06000F7B RID: 3963 RVA: 0x0004B559 File Offset: 0x00049759
	public void SetPlayerDataFloat(string floatName, float value)
	{
		PlayerData.instance.SetFloat(floatName, value);
	}

	// Token: 0x06000F7C RID: 3964 RVA: 0x0004B568 File Offset: 0x00049768
	public void SetPlayerDataString(string stringName, string value)
	{
		PlayerData.instance.SetString(stringName, value);
	}

	// Token: 0x06000F7D RID: 3965 RVA: 0x0004B577 File Offset: 0x00049777
	public void IncrementPlayerDataInt(string intName)
	{
		PlayerData.instance.IncrementInt(intName);
	}

	// Token: 0x06000F7E RID: 3966 RVA: 0x0004B585 File Offset: 0x00049785
	public void DecrementPlayerDataInt(string intName)
	{
		PlayerData.instance.DecrementInt(intName);
	}

	// Token: 0x06000F7F RID: 3967 RVA: 0x0004B593 File Offset: 0x00049793
	public void IntAdd(string intName, int amount)
	{
		PlayerData.instance.IntAdd(intName, amount);
	}

	// Token: 0x06000F80 RID: 3968 RVA: 0x0004B5A2 File Offset: 0x000497A2
	public bool GetPlayerDataBool(string boolName)
	{
		return PlayerData.instance.GetBool(boolName);
	}

	// Token: 0x06000F81 RID: 3969 RVA: 0x0004B5B0 File Offset: 0x000497B0
	public int GetPlayerDataInt(string intName)
	{
		return PlayerData.instance.GetInt(intName);
	}

	// Token: 0x06000F82 RID: 3970 RVA: 0x0004B5BE File Offset: 0x000497BE
	public float GetPlayerDataFloat(string floatName)
	{
		return PlayerData.instance.GetFloat(floatName);
	}

	// Token: 0x06000F83 RID: 3971 RVA: 0x0004B5CC File Offset: 0x000497CC
	public string GetPlayerDataString(string stringName)
	{
		return PlayerData.instance.GetString(stringName);
	}

	// Token: 0x06000F84 RID: 3972 RVA: 0x0004B5DA File Offset: 0x000497DA
	public void SetPlayerDataVector3(string vectorName, Vector3 value)
	{
		PlayerData.instance.SetVector3(vectorName, value);
	}

	// Token: 0x06000F85 RID: 3973 RVA: 0x0004B5E9 File Offset: 0x000497E9
	public Vector3 GetPlayerDataVector3(string vectorName)
	{
		return PlayerData.instance.GetVector3(vectorName);
	}

	// Token: 0x06000F86 RID: 3974 RVA: 0x0004B5F7 File Offset: 0x000497F7
	public T GetPlayerDataVariable<T>(string fieldName)
	{
		return PlayerData.instance.GetVariable<T>(fieldName);
	}

	// Token: 0x06000F87 RID: 3975 RVA: 0x0004B605 File Offset: 0x00049805
	public void SetPlayerDataVariable<T>(string fieldName, T value)
	{
		PlayerData.instance.SetVariable<T>(fieldName, value);
	}

	public class SceneLoadInfo
	{
		public virtual void NotifyFetchComplete()
		{
		}

		public virtual bool IsReadyToActivate()
		{
			return true;
		}

		public virtual void NotifyFinished()
		{
		}

		public bool IsFirstLevelForPlayer;
		public string SceneName;
		public GatePosition? HeroLeaveDirection;
		public string EntryGateName;
		public float EntryDelay;
		public bool PreventCameraFadeOut;
		public bool WaitForSceneTransitionCameraFade;
		public GameManager.SceneLoadVisualizations Visualization;
		public bool AlwaysUnloadUnusedAssets;
		public bool forceWaitFetch;
	}

	public enum SceneLoadVisualizations
	{
		// Token: 0x04002FB9 RID: 12217
		Default,
		// Token: 0x04002FBA RID: 12218
		Custom = -1,
		// Token: 0x04002FBB RID: 12219
		Dream = 1,
		// Token: 0x04002FBC RID: 12220
		Colosseum,
		// Token: 0x04002FBD RID: 12221
		GrimmDream,
		// Token: 0x04002FBE RID: 12222
		ContinueFromSave,
		// Token: 0x04002FBF RID: 12223
		GodsAndGlory
	}
}*/
