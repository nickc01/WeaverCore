using GlobalEnums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    internal Rect SceneDimensions { get; private set; }
    public bool TimeSlowed => timeSlowedCount > 0;

    public CameraController cameraCtrl { get; private set; }

    public HeroController hero_ctrl { get; private set; }

    public global::SceneManager sm { get; private set; }

    public float PlayTime => sessionStartTime + sessionPlayTimer;

    public bool RespawningHero { get; set; }

    public event GameManager.SavePersistentState SavePersistentObjects;

    public event GameManager.ResetSemiPersistentState ResetSemiPersistentObjects;

    public event GameManager.DestroyPooledObjects DestroyPersonalPools;

    public event GameManager.UnloadLevel UnloadingLevel;

    public event GameManager.RefreshLanguage RefreshLanguageText;

    public event GameManager.RefreshParticles RefreshParticleLevel;

    public event GameManager.BossLoad OnLoadedBoss;

    public event GameManager.EnterSceneEvent OnFinishedEnteringScene;

    public event GameManager.SceneTransitionFinishEvent OnFinishedSceneTransition;

    public bool IsInSceneTransition { get; private set; }

    public bool HasFinishedEnteringScene => hasFinishedEnteringScene;

    public bool IsLoadingSceneTransition => isLoading;

    public GameManager.SceneLoadVisualizations LoadVisualization => loadVisualization;

    public float CurrentLoadDuration
    {
        get
        {
            if (!isLoading)
            {
                return 0f;
            }
            return currentLoadDuration;
        }
    }

    public bool IsUsingCustomLoadAnimation => isUsingCustomLoadAnimation;

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

                }
                else if (Application.isPlaying)
                {
                    UnityEngine.Object.DontDestroyOnLoad(GameManager._instance.gameObject);
                }
            }
            return GameManager._instance;
        }
    }

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

    private static bool atteptedEditorLoad = false;

    private static void LoadGameManagerEditor()
    {
        atteptedEditorLoad = true;
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("GameManager");
        foreach (string id in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(id);
            if (path.Contains("WeaverCore.Editor"))
            {
                GameObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset != null)
                {
                    _instance = GameObject.Instantiate(asset).GetComponentInChildren<GameManager>();
                }
            }
        }
#endif
    }

    private void Awake()
    {
        gameObject.tag = "GameManager";
        if (GameManager._instance == null)
        {
            GameManager._instance = this;
            UnityEngine.Object.DontDestroyOnLoad(this);
            SetupGameRefs();
            return;
        }
        if (this != GameManager._instance)
        {
            UnityEngine.Object.Destroy(base.gameObject);
            return;
        }
        SetupGameRefs();
    }

    private void Start()
    {
        if (this == GameManager._instance)
        {
            SetupStatusModifiers();
            //LevelActivated(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }

    protected void Update()
    {
        if (isLoading)
        {
            currentLoadDuration += Time.unscaledDeltaTime;
        }
        else
        {
            currentLoadDuration = 0f;
        }
        IncreaseGameTimer(ref sessionPlayTimer);
        UpdateEngagement();
    }

    private void UpdateEngagement()
    {
        if (gameState == GameState.MAIN_MENU)
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
        else if ((gameState == GameState.PLAYING || gameState == GameState.PAUSED)/* && Platform.Current.EngagementState != Platform.EngagementStates.Engaged*/ && !isEmergencyReturningToMenu)
        {
            //Debug.LogFormat("Engagement is not set up right, returning to menu...", Array.Empty<object>());
            /*this.EmergencyReturnToMenu(delegate
			{
			});*/
        }
    }

    private void LevelActivated(Scene sceneFrom, Scene sceneTo)
    {
        if (this == GameManager._instance)
        {
            if (!waitForManualLevelStart)
            {
                Debug.LogFormat(this, "Performing automatic level start.", Array.Empty<object>());
                if (startedOnThisScene && IsGameplayScene())
                {
                    tilemapDirty = true;
                }
                SetupSceneRefs(true);
                //Debug.LogError("BEGIN SCENE - AAA");
                BeginScene();
                OnNextLevelReady();
                return;
            }
            Debug.LogFormat(this, "Deferring level start (marked as manual).", Array.Empty<object>());
        }
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= LevelActivated;
    }

    private void OnApplicationQuit()
    {
        //this.orig_OnApplicationQuit();
        ModHooks.OnApplicationQuit();
    }

    public void BeginSceneTransition(GameManager.SceneLoadInfo info)
    {
        //info.SceneName = ModHooks.BeforeSceneLoad(info.SceneName);
        orig_BeginSceneTransition(info);
    }

    public static event GameManager.SceneTransitionBeganDelegate SceneTransitionBegan;

    private IEnumerator BeginSceneTransitionRoutine(GameManager.SceneLoadInfo info)
    {
        if (sceneLoad != null)
        {
            Debug.LogErrorFormat(this, "Cannot scene transition to {0}, while a scene transition is in progress", new object[]
            {
                info.SceneName
            });
            yield break;
        }
        IsInSceneTransition = true;
        sceneLoad = new SceneLoad(this, info.SceneName);
        isLoading = true;
        loadVisualization = info.Visualization;
        if (hero_ctrl != null)
        {
            if (hero_ctrl.cState.superDashing)
            {
                hero_ctrl.exitedSuperDashing = true;
            }
            if (hero_ctrl.cState.spellQuake)
            {
                hero_ctrl.exitedQuake = true;
            }
            //this.hero_ctrl.proxyFSM.SendEvent("HeroCtrl-LeavingScene");
            hero_ctrl.SetHeroParent(null);
        }
        if (!info.IsFirstLevelForPlayer)
        {
            NoLongerFirstGame();
        }
        SaveLevelState();
        SetState(GameState.EXITING_LEVEL);
        entryGateName = (info.EntryGateName ?? "");
        targetScene = info.SceneName;
        if (hero_ctrl != null)
        {
            hero_ctrl.LeaveScene(info.HeroLeaveDirection);
        }
        if (!info.PreventCameraFadeOut)
        {
            cameraCtrl.FreezeInPlace(true);
            cameraCtrl.FadeOut(CameraFadeType.LEVEL_TRANSITION);
        }
        tilemapDirty = true;
        startedOnThisScene = false;
        nextSceneName = info.SceneName;
        waitForManualLevelStart = true;
        if (UnloadingLevel != null)
        {
            UnloadingLevel();
        }
        string lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        sceneLoad.FetchComplete += delegate ()
        {
            info.NotifyFetchComplete();
        };
        sceneLoad.WillActivate += delegate ()
        {
            if (DestroyPersonalPools != null)
            {
                DestroyPersonalPools();
            }
            entryDelay = info.EntryDelay;
        };
        sceneLoad.ActivationComplete += delegate ()
        {
            UnityEngine.SceneManagement.SceneManager.UnloadScene(lastSceneName);
            RefreshTilemapInfo(info.SceneName);
            sceneLoad.IsUnloadAssetsRequired = (info.AlwaysUnloadUnusedAssets || IsUnloadAssetsRequired(lastSceneName, info.SceneName));
            bool flag2 = true;
            if (!sceneLoad.IsUnloadAssetsRequired)
            {
                float? beginTime = sceneLoad.BeginTime;
                /*if (beginTime != null && Time.realtimeSinceStartup - beginTime.Value > Platform.Current.MaximumLoadDurationForNonCriticalGarbageCollection && this.sceneLoadsWithoutGarbageCollect < Platform.Current.MaximumSceneTransitionsWithoutNonCriticalGarbageCollection)
				{
					flag2 = false;
				}*/
            }
            if (flag2)
            {
                sceneLoadsWithoutGarbageCollect = 0;
            }
            else
            {
                sceneLoadsWithoutGarbageCollect++;
            }
            sceneLoad.IsGarbageCollectRequired = flag2;
            //Platform.Current.FlushSocialEvents();
        };
        sceneLoad.Complete += delegate ()
        {
            Debug.LogError("BEGIN SCENE - BBB");
            SetupSceneRefs(false);
            BeginScene();
            /*if (this.gameMap != null)
			{
				this.gameMap.GetComponent<GameMap>().LevelReady();
			}*/
        };
        sceneLoad.Finish += delegate ()
        {
            sceneLoad = null;
            isLoading = false;
            waitForManualLevelStart = false;
            info.NotifyFinished();
            OnNextLevelReady();
            IsInSceneTransition = false;
            if (OnFinishedSceneTransition != null)
            {
                OnFinishedSceneTransition();
            }
        };
        if (GameManager.SceneTransitionBegan != null)
        {
            try
            {
                GameManager.SceneTransitionBegan(sceneLoad);
            }
            catch (Exception exception)
            {
                Debug.LogError("Exception in responders to GameManager.SceneTransitionBegan. Attempting to continue load regardless.");
                Debug.LogException(exception);
            }
        }
        sceneLoad.IsFetchAllowed = (!info.forceWaitFetch && (/*Platform.Current.FetchScenesBeforeFade || */info.PreventCameraFadeOut));
        sceneLoad.IsActivationAllowed = false;
        sceneLoad.Begin();
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
        sceneLoad.IsFetchAllowed = true;
        sceneLoad.IsActivationAllowed = true;
        yield break;
    }

    public IEnumerator TransitionScene(TransitionPoint gate)
    {
        Debug.LogError("TransitionScene(TransitionPoint) is no longer supported");
        callingGate = gate;
        if (hero_ctrl.cState.superDashing)
        {
            hero_ctrl.exitedSuperDashing = true;
        }
        if (hero_ctrl.cState.spellQuake)
        {
            hero_ctrl.exitedQuake = true;
        }
        //this.hero_ctrl.GetComponent<PlayMakerFSM>().SendEvent("HeroCtrl-LeavingScene");
        NoLongerFirstGame();
        SaveLevelState();
        SetState(GameState.EXITING_LEVEL);
        entryGateName = gate.entryPoint;
        targetScene = gate.targetScene;
        hero_ctrl.LeaveScene(new GatePosition?(gate.GetGatePosition()));
        cameraCtrl.FreezeInPlace(true);
        cameraCtrl.FadeOut(CameraFadeType.LEVEL_TRANSITION);
        hasFinishedEnteringScene = false;
        yield return new WaitForSeconds(0.5f);
        LeftScene(true);
        yield break;
    }

    public void ChangeToScene(string targetScene, string entryGateName, float pauseBeforeEnter)
    {
        if (hero_ctrl != null)
        {
            //this.hero_ctrl.proxyFSM.SendEvent("HeroCtrl-LeavingScene");
            hero_ctrl.transform.SetParent(null);
        }
        NoLongerFirstGame();
        SaveLevelState();
        SetState(GameState.EXITING_LEVEL);
        this.entryGateName = entryGateName;
        this.targetScene = targetScene;
        entryDelay = pauseBeforeEnter;
        cameraCtrl.FreezeInPlace(false);
        if (hero_ctrl != null)
        {
            hero_ctrl.ResetState();
        }
        LeftScene(false);
    }

    public void WarpToDreamGate()
    {
        entryGateName = "dreamGate";
        targetScene = playerData.GetString("dreamGateScene");
        entryDelay = 0f;
        cameraCtrl.FreezeInPlace(false);
        BeginSceneTransition(new GameManager.SceneLoadInfo
        {
            AlwaysUnloadUnusedAssets = true,
            EntryGateName = "dreamGate",
            PreventCameraFadeOut = true,
            SceneName = playerData.GetString("dreamGateScene"),
            Visualization = GameManager.SceneLoadVisualizations.Dream
        });
    }

    public void LeftScene(bool doAdditiveLoad = false)
    {
        UnityEngine.SceneManagement.SceneManager.GetSceneByName(targetScene);
        if (doAdditiveLoad)
        {
            base.StartCoroutine(LoadSceneAdditive(targetScene));
            return;
        }
        LoadScene(targetScene);
    }

    public IEnumerator PlayerDead(float waitTime)
    {
        ModHooks.OnBeforePlayerDead();
        yield return orig_PlayerDead(waitTime);
        ModHooks.OnAfterPlayerDead();
        yield break;
    }

    public IEnumerator PlayerDeadFromHazard(float waitTime)
    {
        cameraCtrl.FreezeInPlace(true);
        NoLongerFirstGame();
        SaveLevelState();
        yield return new WaitForSeconds(waitTime);
        cameraCtrl.FadeOut(CameraFadeType.HERO_HAZARD_DEATH);
        yield return new WaitForSeconds(0.8f);
        //PlayMakerFSM.BroadcastEvent("HAZARD RELOAD");
        HazardRespawn();
        yield break;
    }

    public void ReadyForRespawn(bool isFirstLevelForPlayer)
    {
        RespawningHero = true;
        BeginSceneTransition(new GameManager.SceneLoadInfo
        {
            PreventCameraFadeOut = true,
            WaitForSceneTransitionCameraFade = false,
            EntryGateName = "",
            SceneName = playerData.GetString("respawnScene"),
            Visualization = (isFirstLevelForPlayer ? GameManager.SceneLoadVisualizations.ContinueFromSave : GameManager.SceneLoadVisualizations.Default),
            AlwaysUnloadUnusedAssets = true,
            IsFirstLevelForPlayer = isFirstLevelForPlayer
        });
    }

    public void HazardRespawn()
    {
        hazardRespawningHero = true;
        entryGateName = "";
        cameraCtrl.ResetStartTimer();
        cameraCtrl.camTarget.mode = CameraTarget.TargetMode.FOLLOW_HERO;
        EnterHero(false);
    }

    public void TimePasses()
    {
        playerData.SetBool("bankerTheftCheck", true);
        if (playerData.GetBool("defeatedDungDefender") && !playerData.GetBool("dungDefenderEncounterReady"))
        {
            playerData.SetBoolSwappedArgs(true, "dungDefenderEncounterReady");
        }
        if (playerData.GetBool("nailsmithCliff") && !playerData.GetBool("nailsmithKilled"))
        {
            playerData.SetBoolSwappedArgs(true, "nailsmithSpared");
        }
        if (playerData.GetBool("hasDashSlash") && playerData.GetBool("nailsmithSpared"))
        {
            playerData.SetBoolSwappedArgs(true, "nailsmithSheo");
        }
        if (playerData.GetBool("brettaRescued") && sm.mapZone.ToString() != "TOWN")
        {
            if (UnityEngine.Random.Range(0f, 1f) >= 0.5f)
            {
                playerData.SetIntSwappedArgs(0, "brettaPosition");
            }
            else
            {
                playerData.SetIntSwappedArgs(1, "brettaPosition");
            }
            if (playerData.GetBool("brettaSeenBench") && !playerData.GetBool("brettaSeenBenchDiary"))
            {
                playerData.SetBoolSwappedArgs(true, "brettaSeenBenchDiary");
            }
            if (playerData.GetBool("brettaSeenBed") && !playerData.GetBool("brettaSeenBedDiary"))
            {
                playerData.SetBoolSwappedArgs(true, "brettaSeenBedDiary");
            }
        }
        if (playerData.GetBool("legEaterLeft") && playerData.GetBool("defeatedNightmareGrimm") && sm.mapZone.ToString() != "TOWN" && playerData.GetBool("divineFinalConvo"))
        {
            playerData.SetBoolSwappedArgs(false, "divineInTown");
        }
        if (playerData.GetBool("zoteSpokenCity"))
        {
            playerData.SetBoolSwappedArgs(true, "zoteLeftCity");
        }
        if (playerData.GetBool("finalGrubRewardCollected"))
        {
            playerData.SetBoolSwappedArgs(true, "fatGrubKing");
        }
        if (playerData.GetBool("dungDefenderAwoken"))
        {
            playerData.SetBoolSwappedArgs(true, "dungDefenderLeft");
        }
        if (playerData.GetBool("scaredFlukeHermitEncountered"))
        {
            playerData.SetBoolSwappedArgs(true, "scaredFlukeHermitReturned");
        }
        if (playerData.GetBool("xunFlowerGiven"))
        {
            playerData.SetBoolSwappedArgs(true, "extraFlowerAppear");
        }
        if (playerData.GetBool("nailsmithKilled") && playerData.GetBool("godseekerSpokenAwake"))
        {
            playerData.SetBoolSwappedArgs(true, "nailsmithCorpseAppeared");
        }
    }

    public void FadeSceneIn()
    {
        cameraCtrl.FadeSceneIn();
    }

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
        FadeSceneIn();
        yield break;
    }

    public bool IsGamePaused()
    {
        return gameState == GameState.PAUSED;
    }

    public void SetGameMap(GameObject go_gameMap)
    {
        gameMap = go_gameMap;
    }

    public void CalculateNotchesUsed()
    {
        //this.playerData.CalculateNotchesUsed();
    }

    public string GetLanguageAsString()
    {
        return editorLanguage.ToString();
    }

    public string GetEntryGateName()
    {
        return entryGateName;
    }

    public void SetPlayerDataBool(string boolName, bool value)
    {
        playerData.SetBool(boolName, value);
    }

    public void SetPlayerDataInt(string intName, int value)
    {
        playerData.SetInt(intName, value);
    }

    public void SetPlayerDataFloat(string floatName, float value)
    {
        playerData.SetFloat(floatName, value);
    }

    public void SetPlayerDataString(string stringName, string value)
    {
        playerData.SetString(stringName, value);
    }

    public void IncrementPlayerDataInt(string intName)
    {
        playerData.IncrementInt(intName);
    }

    public void DecrementPlayerDataInt(string intName)
    {
        playerData.DecrementInt(intName);
    }

    public void IntAdd(string intName, int amount)
    {
        playerData.IntAdd(intName, amount);
    }

    public bool GetPlayerDataBool(string boolName)
    {
        return playerData.GetBool(boolName);
    }

    public int GetPlayerDataInt(string intName)
    {
        return playerData.GetInt(intName);
    }

    public float GetPlayerDataFloat(string floatName)
    {
        return playerData.GetFloat(floatName);
    }

    public string GetPlayerDataString(string stringName)
    {
        return playerData.GetString(stringName);
    }

    public void SetPlayerDataVector3(string vectorName, Vector3 value)
    {
        playerData.SetVector3(vectorName, value);
    }

    public Vector3 GetPlayerDataVector3(string vectorName)
    {
        return playerData.GetVector3(vectorName);
    }

    public T GetPlayerDataVariable<T>(string fieldName)
    {
        return playerData.GetVariable<T>(fieldName);
    }

    public void SetPlayerDataVariable<T>(string fieldName, T value)
    {
        playerData.SetVariable<T>(fieldName, value);
    }

    public void EquipCharm(int charmNum)
    {
        //this.playerData.EquipCharm(charmNum);
    }

    public void UnequipCharm(int charmNum)
    {
        //this.playerData.UnequipCharm(charmNum);
    }

    public void RefreshOvercharm()
    {
        if (playerData.GetInt("charmSlotsFilled") > playerData.GetInt("charmSlots"))
        {
            playerData.SetBoolSwappedArgs(true, "overcharmed");
            return;
        }
        playerData.SetBoolSwappedArgs(false, "overcharmed");
    }

    public void UpdateBlueHealth()
    {
        //this.playerData.UpdateBlueHealth();
    }

    public void SetCurrentMapZoneAsRespawn()
    {
        playerData.SetVariableSwappedArgs<MapZone>(sm.mapZone, "mapZone");
    }

    public void SetMapZoneToSpecific(string mapZone)
    {
        object obj = Enum.Parse(typeof(MapZone), mapZone);
        if (obj != null)
        {
            playerData.SetVariableSwappedArgs<MapZone>((MapZone)obj, "mapZone");
            return;
        }
        Debug.LogError("Couldn't convert " + mapZone + " to a MapZone");
    }

    public void StartSoulLimiter()
    {
        //this.playerData.StartSoulLimiter();
    }

    public void EndSoulLimiter()
    {
        //this.playerData.EndSoulLimiter();
    }

    public bool UpdateGameMap()
    {
        return false;
        //return this.playerData.UpdateGameMap();
    }

    public void CheckAllMaps()
    {
        //this.playerData.CheckAllMaps();
    }

    public void AddToScenesVisited(string scene)
    {
        if (!playerData.GetVariable<List<string>>("scenesVisited").Contains(scene))
        {
            playerData.GetVariable<List<string>>("scenesVisited").Add(scene);
        }
    }

    public bool GetIsSceneVisited(string scene)
    {
        return playerData.GetVariable<List<string>>("scenesVisited").Contains(scene);
    }

    public void AddToBenchList()
    {
        if (!playerData.GetVariable<List<string>>("scenesEncounteredBench").Contains(GetSceneNameString()))
        {
            playerData.GetVariable<List<string>>("scenesEncounteredBench").Add(GetSceneNameString());
        }
    }

    public void AddToGrubList()
    {
        if (!playerData.GetVariable<List<string>>("scenesGrubRescued").Contains(GetSceneNameString()))
        {
            playerData.GetVariable<List<string>>("scenesGrubRescued").Add(GetSceneNameString());
            /*if (this.gameMap != null)
			{
				this.gameMap.GetComponent<GameMap>().SetupMap(true);
			}*/
        }
    }

    public void AddToFlameList()
    {
        if (!playerData.GetVariable<List<string>>("scenesFlameCollected").Contains(GetSceneNameString()))
        {
            playerData.GetVariable<List<string>>("scenesFlameCollected").Add(GetSceneNameString());
        }
    }

    public void AddToCocoonList()
    {
        if (!playerData.GetVariable<List<string>>("scenesEncounteredCocoon").Contains(GetSceneNameString()))
        {
            playerData.GetVariable<List<string>>("scenesEncounteredCocoon").Add(GetSceneNameString());
        }
    }

    public void AddToDreamPlantList()
    {
        if (!playerData.GetVariable<List<string>>("scenesEncounteredDreamPlant").Contains(GetSceneNameString()))
        {
            playerData.GetVariable<List<string>>("scenesEncounteredDreamPlant").Add(GetSceneNameString());
        }
    }

    public void AddToDreamPlantCList()
    {
        if (!playerData.GetVariable<List<string>>("scenesEncounteredDreamPlantC").Contains(GetSceneNameString()))
        {
            playerData.GetVariable<List<string>>("scenesEncounteredDreamPlantC").Add(GetSceneNameString());
        }
    }

    public void CountGameCompletion()
    {
        //this.playerData.CountGameCompletion();
    }

    public void CountCharms()
    {
        //this.playerData.CountCharms();
    }

    public void CountJournalEntries()
    {
        //this.playerData.CountJournalEntries();
    }

    public void ActivateTestingCheats()
    {
        //this.playerData.ActivateTestingCheats();
    }

    public void GetAllPowerups()
    {
        //this.playerData.GetAllPowerups();
    }

    public void StoryRecord_death()
    {
    }

    public void StoryRecord_rescueGrub()
    {
    }

    public void StoryRecord_defeatedShade()
    {
    }

    public void StoryRecord_discoveredArea(string areaName)
    {
    }

    public void StoryRecord_travelledToArea(string areaName)
    {
    }

    public void StoryRecord_bankDeposit(int amount)
    {
    }

    public void StoryRecord_bankWithdraw(int amount)
    {
    }

    public void StoryRecord_boughtCorniferMap(string map)
    {
    }

    public void StoryRecord_visited(string visited)
    {
    }

    public void StoryRecord_defeated(string defeated)
    {
    }

    public void StoryRecord_jiji()
    {
    }

    public void StoryRecord_rodeStag(string dest)
    {
    }

    public void StoryRecord_acquired(string item)
    {
    }

    public void StoryRecord_bought(string item)
    {
    }

    public void StoryRecord_quit()
    {
    }

    public void StoryRecord_rest()
    {
    }

    public void StoryRecord_upgradeNail()
    {
    }

    public void StoryRecord_heartPiece()
    {
    }

    public void StoryRecord_maxHealthUp()
    {
    }

    public void StoryRecord_soulPiece()
    {
    }

    public void StoryRecord_maxSoulUp()
    {
    }

    public void StoryRecord_charmsChanged()
    {
    }

    public void StoryRecord_charmEquipped(string charmName)
    {
    }

    public void StoryRecord_start()
    {
    }

    public void AwardAchievement(string key)
    {
        //this.achievementHandler.AwardAchievementToPlayer(key);
    }

    public void QueueAchievement(string key)
    {
        //this.achievementHandler.QueueAchievement(key);
    }

    public void AwardQueuedAchievements()
    {
        //this.achievementHandler.AwardQueuedAchievements();
    }

    public bool IsAchievementAwarded(string key)
    {
        return false;
        //return this.achievementHandler.AchievementWasAwarded(key);
    }

    public void ClearAllAchievements()
    {
        //this.achievementHandler.ResetAllAchievements();
    }

    public void CheckCharmAchievements()
    {
        CountCharms();
        if (playerData.GetBool("hasCharm"))
        {
            AwardAchievement("CHARMED");
        }
        if (playerData.GetInt("charmsOwned") >= 20)
        {
            AwardAchievement("ENCHANTED");
        }
        if (playerData.GetBool("salubraBlessing"))
        {
            AwardAchievement("BLESSED");
        }
    }

    public void CheckGrubAchievements()
    {
        if (playerData.GetInt("grubsCollected") >= 23)
        {
            AwardAchievement("GRUBFRIEND");
        }
        if (playerData.GetInt("grubsCollected") >= 46)
        {
            AwardAchievement("METAMORPHOSIS");
        }
    }

    public void CheckStagStationAchievements()
    {
        if (playerData.GetInt("stationsOpened") >= 4)
        {
            AwardAchievement("STAG_STATION_HALF");
        }
    }

    public void CheckMapAchievement()
    {
        if (playerData.GetBool("mapCrossroads") && playerData.GetBool("mapGreenpath") && playerData.GetBool("mapFogCanyon") && playerData.GetBool("mapRoyalGardens") && playerData.GetBool("mapFungalWastes") && playerData.GetBool("mapCity") && playerData.GetBool("mapWaterways") && playerData.GetBool("mapMines") && playerData.GetBool("mapDeepnest") && playerData.GetBool("mapCliffs") && playerData.GetBool("mapOutskirts") && playerData.GetBool("mapRestingGrounds") && playerData.GetBool("mapAbyss"))
        {
            AwardAchievement("MAP");
        }
    }

    public void CheckJournalAchievements()
    {
        //this.playerData.CountJournalEntries();
        if (playerData.GetInt("journalEntriesCompleted") >= playerData.GetInt("journalEntriesTotal"))
        {
            AwardAchievement("HUNTER_1");
        }
        if (playerData.GetBool("killedHunterMark"))
        {
            AwardAchievement("HUNTER_2");
        }
    }

    public void CheckAllAchievements()
    {
        /*if (!Platform.Current.IsFiringAchievementsFromSavesAllowed)
		{
			return;
		}*/
        CheckMapAchievement();
        CheckStagStationAchievements();
        CheckGrubAchievements();
        CheckCharmAchievements();
        CheckJournalAchievements();
        if (playerData.GetInt("MPReserveMax") > 0)
        {
            AwardAchievement("SOULFUL");
        }
        if (playerData.GetInt("MPReserveMax") == playerData.GetInt("MPReserveCap"))
        {
            AwardAchievement("WORLDSOUL");
        }
        if (playerData.GetInt("maxHealthBase") > 5)
        {
            AwardAchievement("PROTECTED");
        }
        if (playerData.GetInt("maxHealthBase") == playerData.GetInt("maxHealthCap"))
        {
            AwardAchievement("MASKED");
        }
        if (playerData.GetInt("dreamOrbs") >= 600)
        {
            AwardAchievement("ATTUNEMENT");
        }
        if (playerData.GetBool("dreamNailUpgraded"))
        {
            AwardAchievement("AWAKENING");
        }
        if (playerData.GetBool("mothDeparted"))
        {
            AwardAchievement("ASCENSION");
        }
        if (playerData.GetBool("hornet1Defeated"))
        {
            AwardAchievement("HORNET_1");
        }
        if (playerData.GetBool("hornetOutskirtsDefeated"))
        {
            AwardAchievement("HORNET_2");
        }
        if (playerData.GetBool("mageLordDefeated"))
        {
            AwardAchievement("SOUL_MASTER_DEFEAT");
        }
        if (playerData.GetBool("mageLordDreamDefeated"))
        {
            AwardAchievement("DREAM_SOUL_MASTER_DEFEAT");
        }
        if (playerData.GetBool("killedInfectedKnight"))
        {
            AwardAchievement("BROKEN_VESSEL");
        }
        if (playerData.GetBool("infectedKnightDreamDefeated"))
        {
            AwardAchievement("DREAM_BROKEN_VESSEL");
        }
        if (playerData.GetBool("killedDungDefender"))
        {
            AwardAchievement("DUNG_DEFENDER");
        }
        if (playerData.GetBool("falseKnightDreamDefeated"))
        {
            AwardAchievement("DREAM_FK");
        }
        if (playerData.GetBool("killedMantisLord"))
        {
            AwardAchievement("MANTIS_LORDS");
        }
        if (playerData.GetBool("killedJarCollector"))
        {
            AwardAchievement("COLLECTOR");
        }
        if (playerData.GetBool("killedTraitorLord"))
        {
            AwardAchievement("TRAITOR_LORD");
        }
        if (playerData.GetBool("killedWhiteDefender"))
        {
            AwardAchievement("WHITE_DEFENDER");
        }
        if (playerData.GetBool("killedGreyPrince"))
        {
            AwardAchievement("GREY_PRINCE");
        }
        if (playerData.GetBool("hegemolDefeated"))
        {
            AwardAchievement("BEAST");
        }
        if (playerData.GetBool("lurienDefeated"))
        {
            AwardAchievement("WATCHER");
        }
        if (playerData.GetBool("monomonDefeated"))
        {
            AwardAchievement("TEACHER");
        }
        if (playerData.GetBool("colosseumBronzeCompleted"))
        {
            AwardAchievement("COLOSSEUM_1");
        }
        if (playerData.GetBool("colosseumSilverCompleted"))
        {
            AwardAchievement("COLOSSEUM_2");
        }
        if (playerData.GetBool("colosseumGoldCompleted"))
        {
            AwardAchievement("COLOSSEUM_3");
        }
        if (playerData.GetBool("killedGrimm"))
        {
            AwardAchievement("GRIMM");
        }
        if (playerData.GetBool("defeatedNightmareGrimm"))
        {
            AwardAchievement("NIGHTMARE_GRIMM");
        }
        CheckBanishmentAchievement();
        if (playerData.GetBool("nailsmithKilled"))
        {
            AwardAchievement("NAILSMITH_KILL");
        }
        if (playerData.GetBool("nailsmithConvoArt"))
        {
            AwardAchievement("NAILSMITH_SPARE");
        }
        if (playerData.GetBool("mothDeparted"))
        {
            playerData.SetBoolSwappedArgs(true, "hasDreamGate");
        }
        if (playerData.GetBool("hasTramPass"))
        {
            AddToScenesVisited("Deepnest_26_b");
        }
        if (playerData.GetBool("slyRescued"))
        {
            AddToScenesVisited("Crossroads_04_b");
        }
        if (playerData.GetBool("gotCharm_32"))
        {
            AddToScenesVisited("Deepnest_East_14a");
        }
        /*if (this.playerData.GetBool("awardAllAchievements"))
		{
			this.achievementHandler.AwardAllAchievements();
		}*/
    }

    public void CheckBanishmentAchievement()
    {
        if (playerData.GetBool("destroyedNightmareLantern"))
        {
            AwardAchievement("BANISHMENT");
        }
    }

    public void SetStatusRecordInt(string key, int value)
    {
        //Platform.Current.EncryptedSharedData.SetInt(key, value);
    }

    public int GetStatusRecordInt(string key)
    {
        return 0;
        //return Platform.Current.EncryptedSharedData.GetInt(key, 0);
    }

    public void ResetStatusRecords()
    {
        //Platform.Current.EncryptedSharedData.DeleteKey("RecPermadeathMode");
    }

    public void SaveStatusRecords()
    {
        //Platform.Current.EncryptedSharedData.Save();
    }

    public void SetState(GameState newState)
    {
        gameState = newState;
    }

    public void LoadScene(string destScene)
    {
        //destScene = ModHooks.BeforeSceneLoad(destScene);
        orig_LoadScene(destScene);
        //ModHooks.OnSceneChanged(destScene);
    }

    public IEnumerator LoadSceneAdditive(string destScene)
    {
        Debug.Log("Loading " + destScene);
        //destScene = ModHooks.BeforeSceneLoad(destScene);
        tilemapDirty = true;
        startedOnThisScene = false;
        nextSceneName = destScene;
        waitForManualLevelStart = true;
        if (DestroyPersonalPools != null)
        {
            DestroyPersonalPools();
        }
        if (UnloadingLevel != null)
        {
            UnloadingLevel();
        }
        string exitingScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(destScene, LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = true;
        yield return asyncOperation;
        yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(exitingScene);
        //ModHooks.OnSceneChanged(destScene);
        RefreshTilemapInfo(destScene);
        if (IsUnloadAssetsRequired(exitingScene, destScene))
        {
            Debug.LogFormat(this, "Unloading assets due to zone transition", new object[0]);
            yield return Resources.UnloadUnusedAssets();
        }
        //GCManager.Collect();
        Debug.LogError("BEGIN SCENE - CCC");
        SetupSceneRefs(true);
        BeginScene();
        OnNextLevelReady();
        waitForManualLevelStart = false;
        Debug.Log("Done Loading " + destScene);
        yield break;
    }

    public void OnNextLevelReady()
    {
        if (IsGameplayScene())
        {
            SetState(GameState.ENTERING_LEVEL);
            playerData.SetBoolSwappedArgs(false, "disablePause");
            //this.inputHandler.AllowPause();
            //this.inputHandler.StartAcceptingInput();
            EnterHero(true);
            UpdateUIStateFromGameState();
        }
    }

    public void OnWillActivateFirstLevel()
    {
        orig_OnWillActivateFirstLevel();
        ModHooks.OnNewGame();
    }

    public IEnumerator LoadFirstScene()
    {
        yield return new WaitForEndOfFrame();
        OnWillActivateFirstLevel();
        LoadScene("Tutorial_01");
        ModHooks.OnNewGame();
        yield break;
    }

    public void LoadPermadeathUnlockScene()
    {
        if (GetStatusRecordInt("RecPermadeathMode") == 0)
        {
            LoadScene("PermaDeath_Unlock");
            return;
        }
        base.StartCoroutine(ReturnToMainMenu(GameManager.ReturnToMainMenuSaveModes.SaveAndContinueOnFail, null));
    }

    public void LoadMrMushromScene()
    {
        if (playerData.GetInt("mrMushroomState") >= 8)
        {
            LoadScene("Cinematic_MrMushroom");
            return;
        }
        LoadScene("End_Game_Completion");
    }

    public void LoadOpeningCinematic()
    {
        SetState(GameState.CUTSCENE);
        LoadScene("Intro_Cutscene");
    }

    private void PositionHeroAtSceneEntrance()
    {
        Vector2 position = FindEntryPoint(entryGateName, default(Scene)) ?? new Vector2(-20000f, 20000f);
        if (hero_ctrl != null)
        {
            hero_ctrl.transform.SetPosition2D(position);
        }
    }

    private Vector2? FindEntryPoint(string entryPointName, Scene filterScene)
    {
        if (RespawningHero)
        {
            Transform transform = hero_ctrl.LocateSpawnPoint();
            if (transform != null)
            {
                return new Vector2?(transform.transform.position);
            }
            return null;
        }
        else
        {
            if (hazardRespawningHero)
            {
                return new Vector2?(playerData.GetVector3("hazardRespawnLocation"));
            }
            if (entryGateName == "dreamGate")
            {
                return new Vector2?(new Vector2(playerData.GetFloat("dreamGateX"), playerData.GetFloat("dreamGateY")));
            }
            TransitionPoint transitionPoint = FindTransitionPoint(entryPointName, filterScene, true);
            if (transitionPoint != null)
            {
                return new Vector2?((Vector2)transitionPoint.transform.position + transitionPoint.entryOffset);
            }
            return null;
        }
    }

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

    private void EnterHero(bool additiveGateSearch = false)
    {
        if (entryGateName == "door_dreamReturn" && !string.IsNullOrEmpty(playerData.GetString("bossReturnEntryGate")))
        {
            if (GetCurrentMapZone() == MapZone.GODS_GLORY.ToString())
            {
                entryGateName = playerData.GetString("bossReturnEntryGate");
            }
            playerData.SetStringSwappedArgs(string.Empty, "bossReturnEntryGate");
        }
        if (RespawningHero)
        {
            if (needFirstFadeIn)
            {
                base.StartCoroutine(FadeSceneInWithDelay(0.3f));
                needFirstFadeIn = false;
            }
            base.StartCoroutine(hero_ctrl.Respawn());
            FinishedEnteringScene();
            RespawningHero = false;
            return;
        }
        if (hazardRespawningHero)
        {
            base.StartCoroutine(hero_ctrl.HazardRespawn());
            FinishedEnteringScene();
            hazardRespawningHero = false;
            return;
        }
        if (entryGateName == "dreamGate")
        {
            hero_ctrl.EnterSceneDreamGate();
            return;
        }
        if (startedOnThisScene)
        {
            if (IsGameplayScene())
            {
                FinishedEnteringScene();
                FadeSceneIn();
            }
            return;
        }
        SetState(GameState.ENTERING_LEVEL);
        if (string.IsNullOrEmpty(entryGateName))
        {
            Debug.LogError("No entry gate has been defined in the Game Manager, unable to move hero into position.");
            FinishedEnteringScene();
            return;
        }
        if (additiveGateSearch)
        {
            if (verboseMode)
            {
                Debug.Log("Searching for entry gate " + entryGateName + " in the next scene: " + nextSceneName);
            }
            foreach (GameObject gameObject in UnityEngine.SceneManagement.SceneManager.GetSceneByName(nextSceneName).GetRootGameObjects())
            {
                TransitionPoint component = gameObject.GetComponent<TransitionPoint>();
                if (component != null && component.name == entryGateName)
                {
                    if (verboseMode)
                    {
                        Debug.Log("SUCCESS - Found as root object");
                    }
                    base.StartCoroutine(hero_ctrl.EnterScene(component, entryDelay));
                    return;
                }
                if (gameObject.name == "_Transition Gates")
                {
                    TransitionPoint[] componentsInChildren = gameObject.GetComponentsInChildren<TransitionPoint>();
                    for (int j = 0; j < componentsInChildren.Length; j++)
                    {
                        if (componentsInChildren[j].name == entryGateName)
                        {
                            if (verboseMode)
                            {
                                Debug.Log("SUCCESS - Found in _Transition Gates folder");
                            }
                            base.StartCoroutine(hero_ctrl.EnterScene(componentsInChildren[j], entryDelay));
                            return;
                        }
                    }
                }
                TransitionPoint[] componentsInChildren2 = gameObject.GetComponentsInChildren<TransitionPoint>();
                for (int k = 0; k < componentsInChildren2.Length; k++)
                {
                    if (componentsInChildren2[k].name == entryGateName)
                    {
                        if (verboseMode)
                        {
                            Debug.Log("SUCCESS - Found as a child of a random scene object, can't win em all");
                        }
                        base.StartCoroutine(hero_ctrl.EnterScene(componentsInChildren2[k], entryDelay));
                        return;
                    }
                }
            }
            Debug.LogError("Searching in next scene for TransitionGate failed.");
            return;
        }
        GameObject gameObject2 = GameObject.Find(entryGateName);
        if (gameObject2 != null)
        {
            TransitionPoint component2 = gameObject2.GetComponent<TransitionPoint>();
            base.StartCoroutine(hero_ctrl.EnterScene(component2, entryDelay));
            return;
        }
        Debug.LogError(string.Concat(new string[]
        {
            "No entry point found with the name \"",
            entryGateName,
            "\" in this scene (",
            sceneName,
            "). Unable to move hero into position, trying alternative gates..."
        }));
        TransitionPoint[] array = UnityEngine.Object.FindObjectsOfType<TransitionPoint>();
        if (array != null)
        {
            base.StartCoroutine(hero_ctrl.EnterScene(array[0], entryDelay));
            return;
        }
        Debug.LogError("Could not find any gates in this scene. Trying last ditch spawn...");
        //this.hero_ctrl.transform.SetPosition2D((float)this.tilemap.width / 2f, (float)this.tilemap.height / 2f);
        hero_ctrl.EnterSceneDreamGate();
    }

    public void FinishedEnteringScene()
    {
        SetState(GameState.PLAYING);
        entryDelay = 0f;
        hasFinishedEnteringScene = true;
        if (OnFinishedEnteringScene != null)
        {
            OnFinishedEnteringScene();
        }
    }

    private void SetupGameRefs()
    {
        playerData = PlayerData.instance;
        //this.sceneData = SceneData.instance;
        gameCams = GameCameras.instance;
        cameraCtrl = gameCams.cameraController;
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
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += LevelActivated;
        //Platform.Current.SetDisengageHandler(this);
    }

    public void SetupSceneRefs(bool refreshTilemapInfo)
    {
        orig_SetupSceneRefs(refreshTilemapInfo);
        if (IsGameplayScene())
        {
            /*tk2dSpriteAnimator component = GameCameras.instance.soulOrbFSM.gameObject.transform.Find("SoulOrb_fill").gameObject.transform.Find("Liquid").gameObject.GetComponent<tk2dSpriteAnimator>();
			component.GetClipByName("Fill").fps = 15.749999f;
			component.GetClipByName("Idle").fps = 10.5f;
			component.GetClipByName("Shrink").fps = 15.749999f;
			component.GetClipByName("Drain").fps = 31.499998f;*/
        }
    }

    public void SetupHeroRefs()
    {
        hero_ctrl = HeroController.instance;
        //this.heroLight = GameObject.FindGameObjectWithTag("HeroLightMain").GetComponent<SpriteRenderer>();
    }

    public void BeginScene()
    {
        //Debug.LogError("GAME MANAGER BEGIN SCENE");
        //this.inputHandler.SceneInit();
        //this.ui.SceneInit();
        if (hero_ctrl)
        {
            hero_ctrl.SceneInit();
        }
        gameCams.SceneInit();
        if (IsMenuScene())
        {
            SetState(GameState.MAIN_MENU);
            UpdateUIStateFromGameState();
            //Platform.Current.SetSocialPresence("IN_MENU", true);
            return;
        }
        if (IsGameplayScene())
        {
            if ((!Application.isEditor && !Debug.isDebugBuild) || Time.renderedFrameCount > 3)
            {
                PositionHeroAtSceneEntrance();
            }
            if (sm != null)
            {
                //Platform.Current.SetSocialPresence("EXPLORING_" + this.sm.mapZone.ToString(), true);
                return;
            }
        }
        else
        {
            if (IsNonGameplayScene())
            {
                SetState(GameState.CUTSCENE);
                UpdateUIStateFromGameState();
                return;
            }
            Debug.LogError("GM - Scene type is not set to a standard scene type.");
            UpdateUIStateFromGameState();
        }
    }

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

    public void SkipCutscene()
    {
        base.StartCoroutine(SkipCutsceneNoMash());
    }

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

    public void NoLongerFirstGame()
    {
        if (playerData.GetBool("isFirstGame"))
        {
            playerData.SetBoolSwappedArgs(false, "isFirstGame");
        }
    }

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

    public void MatchBackerCreditsSetting()
    {
        /*if (this.gameSettings.backerCredits > 0)
		{
			this.playerData.SetBoolSwappedArgs(true, "backerCredits");
			return;
		}*/
        playerData.SetBoolSwappedArgs(false, "backerCredits");
    }

    public void RefreshLocalization()
    {
        if (RefreshLanguageText != null)
        {
            RefreshLanguageText();
        }
    }

    public void RefreshParticleSystems()
    {
        if (RefreshParticleLevel != null)
        {
            RefreshParticleLevel();
        }
    }

    public void ApplyNativeInput()
    {
    }

    public void EnablePermadeathMode()
    {
        SetStatusRecordInt("RecPermadeathMode", 1);
    }

    public string GetCurrentMapZone()
    {
        return sm.mapZone.ToString();
    }

    public float GetSceneWidth()
    {
        if (IsGameplayScene())
        {
            return sceneWidth;
        }
        return 0f;
    }

    public float GetSceneHeight()
    {
        if (IsGameplayScene())
        {
            return sceneHeight;
        }
        return 0f;
    }

    public GameObject GetSceneManager()
    {
        return sm.gameObject;
    }

    public string GetFormattedMapZoneString(MapZone mapZone)
    {
        return null;
        //return Language.Get(mapZone.ToString(), "Map Zones");
    }

    public void UpdateSceneName()
    {
        sceneName = GameManager.GetBaseSceneName(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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

    public string GetSceneNameString()
    {
        UpdateSceneName();
        return sceneName;
    }

    /*private static tk2dTileMap GetTileMap(GameObject gameObject)
	{
		if (gameObject.CompareTag("TileMap"))
		{
			return gameObject.GetComponent<tk2dTileMap>();
		}
		return null;
	}*/

    private static Type FindType(string assemblyName, string typeName)
    {
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name == assemblyName)
            {
                return asm.GetType(typeName);
            }
        }
        return null;
    }

    public void RefreshTilemapInfo(string targetScene)
    {
        //Debug.Log("Target Scene = " + targetScene);
        Type sceneManagerType = FindType("WeaverCore", "WeaverCore.Components.WeaverSceneManager");
        PropertyInfo sceneDimProp = sceneManagerType.GetProperty("SceneDimensions");

        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene.name == targetScene)
            {
                foreach (GameObject gm in scene.GetRootGameObjects())
                {
                    //Debug.Log("Root Object = " + gm.gameObject);
                    Component manager = gm.GetComponent(sceneManagerType);
                    if (manager != null)
                    {
                        //Debug.Log("Found Scene Manager = " + gm.name);
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

        GameObject placeHolderObject = new GameObject("PLACEHOLDER SCENE MANAGER");

        Component placeholderManager = placeHolderObject.AddComponent(sceneManagerType);

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

    public void SaveLevelState()
    {
        if (SavePersistentObjects != null)
        {
            SavePersistentObjects();
        }
    }

    public void ResetSemiPersistentItems()
    {
        /*if (this.ResetSemiPersistentObjects != null)
		{
			this.ResetSemiPersistentObjects();
		}
		this.sceneData.ResetSemiPersistentItems();*/
    }

    public bool IsMenuScene()
    {
        UpdateSceneName();
        return sceneName == "Menu_Title";
    }

    public bool IsTitleScreenScene()
    {
        UpdateSceneName();
        return string.Compare(sceneName, "Title_Screens", true) == 0;
    }

    public bool IsGameplayScene()
    {
        UpdateSceneName();
        return !IsNonGameplayScene();
    }

    public bool IsNonGameplayScene()
    {
        UpdateSceneName();
        return IsCinematicScene() || sceneName == "Knight Pickup" || sceneName == "Pre_Menu_Intro" || sceneName == "Menu_Title" || sceneName == "End_Credits" || sceneName == "Menu_Credits" || sceneName == "Cutscene_Boss_Door" || sceneName == "PermaDeath_Unlock" || sceneName == "GG_Unlock" || sceneName == "GG_End_Sequence" || sceneName == "End_Game_Completion" || sceneName == "BetaEnd" || sceneName == "PermaDeath" || sceneName == "GG_Entrance_Cutscene" || sceneName == "GG_Boss_Door_Entrance";
    }

    public bool IsCinematicScene()
    {
        UpdateSceneName();
        return sceneName == "Intro_Cutscene_Prologue" || sceneName == "Opening_Sequence" || sceneName == "Prologue_Excerpt" || sceneName == "Intro_Cutscene" || sceneName == "Cinematic_Stag_travel" || sceneName == "PermaDeath" || sceneName == "Cinematic_Ending_A" || sceneName == "Cinematic_Ending_B" || sceneName == "Cinematic_Ending_C" || sceneName == "Cinematic_Ending_D" || sceneName == "Cinematic_Ending_E" || sceneName == "Cinematic_MrMushroom" || sceneName == "BetaEnd";
    }

    public bool IsStagTravelScene()
    {
        UpdateSceneName();
        return sceneName == "Cinematic_Stag_travel";
    }

    public bool IsBetaEndScene()
    {
        UpdateSceneName();
        return sceneName == "BetaEnd";
    }

    public bool IsTutorialScene()
    {
        UpdateSceneName();
        return sceneName == "Tutorial_01";
    }

    public bool IsTestScene()
    {
        UpdateSceneName();
        return sceneName.Contains("test");
    }

    public bool IsBossDoorScene()
    {
        UpdateSceneName();
        return sceneName == "Cutscene_Boss_Door";
    }

    public bool ShouldKeepHUDCameraActive()
    {
        UpdateSceneName();
        return sceneName == "GG_Entrance_Cutscene" || sceneName == "GG_Boss_Door_Entrance" || sceneName == "GG_End_Sequence" || sceneName == "Cinematic_Ending_D";
    }

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

    public void HasSaveFile(int saveSlot, Action<bool> callback)
    {
        //Platform.Current.IsSaveSlotInUse(saveSlot, callback);
    }

    public void SaveGame()
    {
        SaveGame(delegate (bool didSave)
        {
        });
    }

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

    public void SaveGame(Action<bool> callback)
    {
        SaveGame(profileID, callback);
    }

    private void ResetGameTimer()
    {
        sessionPlayTimer = 0f;
        sessionStartTime = playerData.GetFloat("playTime");
    }

    public void IncreaseGameTimer(ref float timer)
    {
        if ((gameState == GameState.PLAYING || gameState == GameState.ENTERING_LEVEL || gameState == GameState.EXITING_LEVEL) && Time.unscaledDeltaTime < 1f)
        {
            timer += Time.unscaledDeltaTime;
        }
    }

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

    public void LoadGameFromUI(int saveSlot)
    {
        base.StartCoroutine(LoadGameFromUIRoutine(saveSlot));
    }

    private IEnumerator LoadGameFromUIRoutine(int saveSlot)
    {
        //this.ui.ContinueGame();
        bool finishedLoading = false;
        bool successfullyLoaded = false;
        LoadGame(saveSlot, delegate (bool didLoad)
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
            ContinueGame();
        }
        else
        {
            //this.ui.UIGoToMainMenu();
        }
        yield break;
    }

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

    public void ClearSaveFile(int saveSlot, Action<bool> callback)
    {
        /*ModHooks.OnSavegameClear(saveSlot);
		this.orig_ClearSaveFile(saveSlot, callback);
		ModHooks.OnAfterSaveGameClear(saveSlot);*/
    }

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

    public IEnumerator PauseGameToggleByMenu()
    {
        yield return null;
        IEnumerator iterator = PauseGameToggle();
        while (iterator.MoveNext())
        {
            object obj = iterator.Current;
            yield return obj;
        }
        yield break;
    }

    public IEnumerator PauseGameToggle()
    {
        if (!TimeSlowed)
        {
            if (/*!this.playerData.disablePause && */gameState == GameState.PLAYING)
            {
                //this.gameCams.StopCameraShake();
                //this.inputHandler.PreventPause();
                //this.inputHandler.StopUIInput();
                //this.actorSnapshotPaused.TransitionTo(0f);
                isPaused = true;
                SetState(GameState.PAUSED);
                //this.ui.AudioGoToPauseMenu(0.2f);
                //this.ui.SetState(UIState.PAUSED);
                if (HeroController.instance != null)
                {
                    HeroController.instance.Pause();
                }
                //this.gameCams.MoveMenuToHUDCamera();
                SetTimeScale(0f);
                yield return new WaitForSecondsRealtime(0.8f);
                //this.inputHandler.AllowPause();
            }
            else if (gameState == GameState.PAUSED)
            {
                //this.gameCams.ResumeCameraShake();
                //this.inputHandler.PreventPause();
                //this.actorSnapshotUnpaused.TransitionTo(0f);
                isPaused = false;
                //this.ui.AudioGoToGameplay(0.2f);
                //this.ui.SetState(UIState.PLAYING);
                SetState(GameState.PLAYING);
                if (HeroController.instance != null)
                {
                    HeroController.instance.UnPause();
                }
                //MenuButtonList.ClearAllLastSelected();
                SetTimeScale(1f);
                yield return new WaitForSecondsRealtime(0.8f);
                //this.inputHandler.AllowPause();
            }
        }
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

	private void SetTimeScale(float newTimeScale)
	{
		if (this.timeSlowedCount > 1)
		{
			newTimeScale = Mathf.Min(newTimeScale, TimeController.GenericTimeScale);
		}
		TimeController.GenericTimeScale = ((newTimeScale > 0.01f) ? newTimeScale : 0f);
	}*/

    public void FreezeMoment(int type)
    {
        if (type == 0)
        {
            base.StartCoroutine(FreezeMoment(0.01f, 0.35f, 0.1f, 0f));
        }
        else if (type == 1)
        {
            base.StartCoroutine(FreezeMoment(0.04f, 0.03f, 0.04f, 0f));
        }
        else if (type == 2)
        {
            base.StartCoroutine(FreezeMoment(0.25f, 2f, 0.25f, 0.15f));
        }
        else if (type == 3)
        {
            base.StartCoroutine(FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
        }
        if (type == 4)
        {
            base.StartCoroutine(FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
        }
        if (type == 5)
        {
            base.StartCoroutine(FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
        }
    }

    private object settings = null;
    private FieldInfo disableFreezeF = null;

    public IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
    {
        if (settings == null)
        {
            Type settingsType = FindType("WeaverCore.Editor", "WeaverCore.Editor.GeneralSettings");

            settings = settingsType.GetProperty("Instance").GetValue(null);

            disableFreezeF = settingsType.GetField("DisableGameFreezing");

        }

        if (((bool)disableFreezeF.GetValue(settings)) == true)
        {
            yield break;
        }

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

    public IEnumerator FreezeMomentGC(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
    {
        if (settings == null)
        {
            Type settingsType = FindType("WeaverCore.Editor", "WeaverCore.Editor.GeneralSettings");

            settings = settingsType.GetProperty("Instance").GetValue(null);

            disableFreezeF = settingsType.GetField("DisableGameFreezing");

        }

        if (((bool)disableFreezeF.GetValue(settings)) == true)
        {
            yield break;
        }
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

    public IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, bool runGc = false)
    {
        timeSlowedCount++;
        yield return StartCoroutine(SetTimeScale(0f, rampDownTime));
        for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
        {
            yield return null;
        }
        yield return StartCoroutine(SetTimeScale(1f, rampUpTime));
        timeSlowedCount--;
        yield break;
    }

    public void EnsureSaveSlotSpace(Action<bool> callback)
    {
        //Platform.Current.EnsureSaveSlotSpace(this.profileID, callback);
    }

    public void StartNewGame(bool permadeathMode = false, bool bossRushMode = false)
    {
        if (permadeathMode)
        {
            playerData.SetIntSwappedArgs(1, "permadeathMode");
        }
        else
        {
            playerData.SetIntSwappedArgs(0, "permadeathMode");
        }
        MatchBackerCreditsSetting();
        if (bossRushMode)
        {
            //this.playerData.AddGGPlayerDataOverrides();
            base.StartCoroutine(RunContinueGame());
            return;
        }
        base.StartCoroutine(RunStartNewGame());
    }

    public IEnumerator RunStartNewGame()
    {
        cameraCtrl.FadeOut(CameraFadeType.START_FADE);
        //this.noMusicSnapshot.TransitionTo(2f);
        //this.noAtmosSnapshot.TransitionTo(2f);
        yield return new WaitForSeconds(2.6f);
        //this.ui.MakeMenuLean();
        BeginSceneTransition(new GameManager.SceneLoadInfo
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

    public void ContinueGame()
    {
        MatchBackerCreditsSetting();
        base.StartCoroutine(RunContinueGame());
    }

    public IEnumerator RunContinueGame()
    {
        cameraCtrl.FadeOut(CameraFadeType.START_FADE);
        //this.noMusicSnapshot.TransitionTo(2f);
        //this.noAtmosSnapshot.TransitionTo(2f);
        yield return new WaitForSeconds(2.6f);
        //this.audioManager.ApplyMusicCue(this.noMusicCue, 0f, 0f, false);
        //this.ui.MakeMenuLean();
        isLoading = true;
        SetState(GameState.LOADING);
        loadVisualization = GameManager.SceneLoadVisualizations.Default;
        //SaveDataUpgradeHandler.UpgradeSaveData(ref this.playerData);
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Knight_Pickup", LoadSceneMode.Additive);
        SetupSceneRefs(false);
        yield return null;
        UnityEngine.SceneManagement.SceneManager.UnloadScene("Knight_Pickup");
        needFirstFadeIn = true;
        isLoading = false;
        ReadyForRespawn(true);
        yield break;
    }

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
        if (UnloadingLevel != null)
        {
            try
            {
                UnloadingLevel();
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Error while UnloadingLevel in QuitToMenu, attempting to continue regardless.");
                Debug.LogException(exception);
            }
        }
        if (DestroyPersonalPools != null)
        {
            try
            {
                DestroyPersonalPools();
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

    public void WillTerminateInBackground()
    {
        if (gameState == GameState.PLAYING || gameState == GameState.PAUSED)
        {
            Debug.LogFormat("Saving in background, because we're about to terminate.", Array.Empty<object>());
            SaveGame();
        }
    }

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

    public IEnumerator QuitGame()
    {
        StoryRecord_quit();
        //FSMUtility.SendEventToGameObject(GameObject.Find("Quit Blanker"), "START FADE", false);
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
        yield break;
    }

    public void LoadedBoss()
    {
        if (OnLoadedBoss != null)
        {
            OnLoadedBoss();
        }
    }

    public void DoDestroyPersonalPools()
    {
        if (DestroyPersonalPools != null)
        {
            DestroyPersonalPools();
        }
    }

    public float GetImplicitCinematicVolume()
    {
        //return Mathf.Clamp01(this.gameSettings.masterVolume / 10f) * Mathf.Clamp01(this.gameSettings.soundVolume / 10f);
        return 1f;
    }

    /*[CompilerGenerated]
	public UIManager orig_get_ui()
	{
		return this.< ui > k__BackingField;
	}

	[CompilerGenerated]
	private void orig_set_ui(UIManager value)
	{
		this.< ui > k__BackingField = value;
	}*/

    /*private void orig_OnApplicationQuit()
	{
		if (this.startedLanguageDisabled)
		{
			this.gameConfig.hideLanguageOption = true;
		}
	}*/

    public void orig_LoadScene(string destScene)
    {
        tilemapDirty = true;
        startedOnThisScene = false;
        nextSceneName = destScene;
        if (DestroyPersonalPools != null)
        {
            DestroyPersonalPools();
        }
        if (UnloadingLevel != null)
        {
            UnloadingLevel();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(destScene);
    }

    public void orig_BeginSceneTransition(GameManager.SceneLoadInfo info)
    {
        //this.inventoryFSM.SendEvent("INVENTORY CANCEL");
        if (info.IsFirstLevelForPlayer)
        {
            ResetGameTimer();
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
        base.StartCoroutine(BeginSceneTransitionRoutine(info));
    }

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

    public IEnumerator orig_PlayerDead(float waitTime)
    {
        cameraCtrl.FreezeInPlace(true);
        NoLongerFirstGame();
        ResetSemiPersistentItems();
        bool finishedSaving = false;
        SaveGame(profileID, delegate (bool didSave)
        {
            finishedSaving = true;
        });
        yield return new WaitForSeconds(waitTime);
        cameraCtrl.FadeOut(CameraFadeType.HERO_DEATH);
        yield return new WaitForSeconds(0.8f);
        while (!finishedSaving)
        {
            yield return null;
        }
        ReadyForRespawn(false);
        /*else if (this.playerData.permadeathMode == 2)
		{
			this.LoadScene("PermaDeath");
		}*/
        yield break;
    }

    private static string ModdedSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, string.Format("user{0}.modded.json", slot));
    }

    public void orig_SetupSceneRefs(bool refreshTilemapInfo)
    {
        UpdateSceneName();
        /*if (this.ui == null)
		{
			this.ui = UIManager.instance;
		}*/

        sm = GameObject.FindObjectOfType<SceneManager>();
        if (sm == null)
        {
            Type sceneManagerType = FindType("WeaverCore", "WeaverCore.Components.WeaverSceneManager");
            PropertyInfo sceneDimProp = sceneManagerType.GetProperty("SceneDimensions");

            GameObject placeHolderObject = new GameObject("PLACEHOLDER SCENE MANAGER");

            Component placeholderManager = placeHolderObject.AddComponent(sceneManagerType);

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
        if (IsGameplayScene())
        {
            if (hero_ctrl == null)
            {
                SetupHeroRefs();
            }
            //this.inputHandler.AttachHeroController(this.hero_ctrl);
            if (refreshTilemapInfo)
            {
                RefreshTilemapInfo(sceneName);
            }
            //this.soulOrb_fsm = this.gameCams.soulOrbFSM;
            //this.soulVessel_fsm = this.gameCams.soulVesselFSM;
        }
    }

    public void orig_OnWillActivateFirstLevel()
    {
        HeroController.instance.isEnteringFirstLevel = true;
        entryGateName = "top1";
        SetState(GameState.PLAYING);
        //this.ui.ConfigureMenu();
    }

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
    private SupportedLanguages editorLanguage = SupportedLanguages.EN;

    private bool verboseMode;

    public GameState gameState;

    public bool isPaused;

    private int timeSlowedCount;

    public string sceneName;

    public string nextSceneName;

    public string entryGateName;

    private TransitionPoint callingGate;

    private Vector3 entrySpawnPoint;

    private float entryDelay;

    [HideInInspector]
    public float sceneWidth;

    [HideInInspector]
    public float sceneHeight;

    //public GameConfig gameConfig;

    private GameCameras gameCams;

    //[SerializeField]
    //private AudioManager audioManager;

    //[SerializeField]
    //private InControlManager inControlManagerPrefab;

    //private static InControlManager _spawnedInControlManager;

    //[SerializeField]
    //public GameSettings gameSettings;

    //public TimeScaleIndependentUpdate timeTool;

    [HideInInspector]
    public GameObject gameMap;

    //public PlayMakerFSM inventoryFSM;

    [SerializeField]
    public PlayerData playerData;

    //[SerializeField]
    //public SceneData sceneData;

    public const int NoSaveSlotID = -1;

    [HideInInspector]
    public int profileID;

    private bool needsFlush;

    private bool isEmergencyReturningToMenu;

    private float sessionPlayTimer;

    private float sessionStartTime;

    public bool startedOnThisScene = true;

    private bool hazardRespawningHero;

    private string targetScene;

    private bool tilemapDirty;

    private bool needFirstFadeIn;

    private bool waitForManualLevelStart;

    private bool startedSteamEnabled;

    private bool startedGOGEnabled;

    private bool startedLanguageDisabled;

    //public AudioMixerSnapshot actorSnapshotUnpaused;

    //public AudioMixerSnapshot actorSnapshotPaused;

    //public AudioMixerSnapshot silentSnapshot;

    //public AudioMixerSnapshot noMusicSnapshot;

    //public MusicCue noMusicCue;

    //public AudioMixerSnapshot noAtmosSnapshot;

    private bool hasFinishedEnteringScene;

    //[SerializeField]
    //private WorldInfo worldInfo;

    private bool isLoading;

    private GameManager.SceneLoadVisualizations loadVisualization;

    private float currentLoadDuration;

    private int sceneLoadsWithoutGarbageCollect;

    private bool isUsingCustomLoadAnimation;

    //[SerializeField]
    //private StandaloneLoadingSpinner standaloneLoadingSpinnerPrefab;

    public static GameManager _instance;

    private SceneLoad sceneLoad;

    private static readonly string[] SubSceneNameSuffixes = new string[]
    {
        "_boss_defeated",
        "_boss",
        "_preload"
    };

    private int saveIconShowCounter;

    //private ModSavegameData moddedData;

    //private UIManager _uiInstance;

    public delegate void SavePersistentState();

    public delegate void ResetSemiPersistentState();

    public delegate void DestroyPooledObjects();

    public delegate void UnloadLevel();

    public delegate void RefreshLanguage();

    public delegate void RefreshParticles();

    public delegate void BossLoad();

    public delegate void EnterSceneEvent();

    public delegate void SceneTransitionFinishEvent();

    public enum SceneLoadVisualizations
    {
        Default,
        Custom = -1,
        Dream = 1,
        Colosseum,
        GrimmDream,
        ContinueFromSave,
        GodsAndGlory
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

    public delegate void SceneTransitionBeganDelegate(SceneLoad sceneLoad);

    public enum ReturnToMainMenuSaveModes
    {
        SaveAndCancelOnFail,
        SaveAndContinueOnFail,
        DontSave
    }

    public enum ControllerConnectionStates
    {
        DetachedDevice,
        DummyDevice,
        NullDevice,
        PossiblyConnected,
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

	public void SetPlayerDataInt(string intName, int value)
	{
		PlayerData.instance.SetInt(intName, value);
	}

	public void SetPlayerDataFloat(string floatName, float value)
	{
		PlayerData.instance.SetFloat(floatName, value);
	}

	public void SetPlayerDataString(string stringName, string value)
	{
		PlayerData.instance.SetString(stringName, value);
	}

	public void IncrementPlayerDataInt(string intName)
	{
		PlayerData.instance.IncrementInt(intName);
	}

	public void DecrementPlayerDataInt(string intName)
	{
		PlayerData.instance.DecrementInt(intName);
	}

	public void IntAdd(string intName, int amount)
	{
		PlayerData.instance.IntAdd(intName, amount);
	}

	public bool GetPlayerDataBool(string boolName)
	{
		return PlayerData.instance.GetBool(boolName);
	}

	public int GetPlayerDataInt(string intName)
	{
		return PlayerData.instance.GetInt(intName);
	}

	public float GetPlayerDataFloat(string floatName)
	{
		return PlayerData.instance.GetFloat(floatName);
	}

	public string GetPlayerDataString(string stringName)
	{
		return PlayerData.instance.GetString(stringName);
	}

	public void SetPlayerDataVector3(string vectorName, Vector3 value)
	{
		PlayerData.instance.SetVector3(vectorName, value);
	}

	public Vector3 GetPlayerDataVector3(string vectorName)
	{
		return PlayerData.instance.GetVector3(vectorName);
	}

	public T GetPlayerDataVariable<T>(string fieldName)
	{
		return PlayerData.instance.GetVariable<T>(fieldName);
	}

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
		Default,
		Custom = -1,
		Dream = 1,
		Colosseum,
		GrimmDream,
		ContinueFromSave,
		GodsAndGlory
	}
}*/
