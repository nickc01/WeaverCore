using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using UnityEngine;

public class BossSceneController : MonoBehaviour
{
    public delegate void SetupEventDelegate(BossSceneController self);

    public struct BossHealthDetails
    {
        public int baseHP;

        public int adjustedHP;
    }

    public static BossSceneController Instance;

    public static SetupEventDelegate SetupEvent;

    public Transform heroSpawn;

    [HideInInspector]
    public GameObject transitionPrefab;

    public EventRegister endTransitionEvent;

    public bool doTransitionIn = true;

    public float transitionInHoldTime;

    public bool doTransitionOut = true;

    public float transitionOutHoldTime;

    private bool isTransitioningOut;

    private bool canTransition = true;

    /*[Space]
    [Tooltip("If scene end is handled elsewhere then leave empty. Only assign bosses here if you want the scene to end on HealthManager death event.")]
    public HealthManager[] bosses;*/

    private int bossesLeft;

    public float bossesDeadWaitTime = 5f;

    private int bossLevel;

    private bool endedScene;

    private bool knightDamagedSubscribed;

    private bool restoreBindingsOnDestroy = true;

    public TransitionPoint customExitPoint;

    private bool doTransition = true;

    public static bool IsBossScene => Instance != null;

    public bool HasTransitionedIn { get; private set; }

    public static bool IsTransitioning
    {
        get
        {
            if (!(Instance != null))
            {
                return false;
            }
            return Instance.isTransitioningOut;
        }
    }

    public bool CanTransition
    {
        get
        {
            return canTransition;
        }
        set
        {
            canTransition = value;
        }
    }

    public int BossLevel
    {
        get
        {
            return bossLevel;
        }
        set
        {
            bossLevel = value;
        }
    }

    public string DreamReturnEvent { get; set; }

    //public Dictionary<HealthManager, BossHealthDetails> BossHealthLookup { get; private set; }

    public event Action OnBossesDead;

    public event Action OnBossSceneComplete;

    private void Awake()
    {
        Instance = this;
        //BossHealthLookup = new Dictionary<HealthManager, BossHealthDetails>();
        if (SetupEvent == null)
        {
            BossSequenceController.CheckLoadSequence(this);
            doTransition = false;
        }
        if (SetupEvent != null)
        {
            SetupEventDelegate setupEvent = SetupEvent;
            SetupEvent = null;
            setupEvent(this);
            Setup();
        }
        if ((bool)customExitPoint)
        {
            customExitPoint.OnBeforeTransition += delegate
            {
                restoreBindingsOnDestroy = false;
            };
        }
    }

    private void OnDestroy()
    {
        if (restoreBindingsOnDestroy)
        {
            RestoreBindings();
        }
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private IEnumerator Start()
    {
        if ((bool)GameManager.instance.sm && GameManager.instance.sm.mapZone != MapZone.GODS_GLORY)
        {
            Debug.LogError("SceneManager map zone not set to GODS_GLORY, boss logic and dream death may break!");
        }
        if (doTransition)
        {
            if ((bool)transitionPrefab)
            {
                UnityEngine.Object.Instantiate(transitionPrefab);
            }
            else
            {
                Debug.LogError("Boss Scene Controller has no transition prefab assigned!", this);
            }
            if (doTransitionIn)
            {
                if (transitionInHoldTime > 0f)
                {
                    EventRegister.SendEvent("GG TRANSITION OUT INSTANT");
                    yield return new WaitForSeconds(transitionInHoldTime);
                }
                EventRegister.SendEvent("GG TRANSITION IN");
            }
        }
        HasTransitionedIn = true;
    }

    private void Update()
    {
        float timer = BossSequenceController.Timer;
        GameManager.instance.IncreaseGameTimer(ref timer);
        BossSequenceController.Timer = timer;
    }

    private void Setup()
    {
        /*for (int i = 0; i < bosses.Length; i++)
        {
            if ((bool)bosses[i])
            {
                bossesLeft++;
                bosses[i].OnDeath += delegate
                {
                    bossesLeft--;
                    CheckBossesDead();
                };
            }
        }*/
        if (!BossSequenceController.KnightDamaged && (bool)HeroController.instance)
        {
            HeroController.instance.OnTakenDamage += SetKnightDamaged;
            knightDamagedSubscribed = true;
        }
    }

    private void SetKnightDamaged()
    {
        BossSequenceController.KnightDamaged = true;
    }

    private void CheckBossesDead()
    {
        if (bossesLeft <= 0)
        {
            EndBossScene();
        }
    }

    public void EndBossScene()
    {
        if (!endedScene)
        {
            endedScene = true;
            if (knightDamagedSubscribed)
            {
                HeroController.instance.OnTakenDamage -= SetKnightDamaged;
            }
            if (this.OnBossesDead != null)
            {
                this.OnBossesDead();
            }
            StartCoroutine(EndSceneDelayed());
        }
    }

    private IEnumerator EndSceneDelayed()
    {
        yield return new WaitForSeconds(bossesDeadWaitTime);
        bool waitingForTransition = false;
        if (doTransitionOut)
        {
            if ((bool)endTransitionEvent)
            {
                isTransitioningOut = true;
                waitingForTransition = true;
                endTransitionEvent.OnReceivedEvent += delegate
                {
                    waitingForTransition = false;
                };
            }
            else
            {
                Debug.LogError("Boss Scene controller has no end transition event assigned!", this);
            }
            if (BossSequenceController.IsInSequence)
            {
                if (BossSequenceController.IsLastBossScene)
                {
                    EventRegister.SendEvent("GG TRANSITION FINAL");
                }
                else
                {
                    EventRegister.SendEvent("GG TRANSITION OUT");
                }
            }
            else
            {
                EventRegister.SendEvent("GG TRANSITION OUT STATUE");
            }
        }
        yield return new WaitForSeconds(transitionOutHoldTime);
        while (waitingForTransition || HeroController.instance.cState.hazardRespawning || HeroController.instance.cState.hazardDeath || HeroController.instance.cState.spellQuake || !CanTransition)
        {
            yield return null;
        }
        if (!HeroController.instance.cState.dead && !HeroController.instance.cState.transitioning)
        {
            Debug.Log("Boss scene ended");
            restoreBindingsOnDestroy = false;
            if (this.OnBossSceneComplete != null)
            {
                this.OnBossSceneComplete();
            }
        }
    }

    public void DoDreamReturn()
    {
        
        /*PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(base.gameObject, "Dream Return");
        if ((bool)playMakerFSM)
        {
            playMakerFSM.SendEvent(DreamReturnEvent);
        }*/
    }

    public void ApplyBindings()
    {
        BossSequenceController.ApplyBindings();
    }

    public void RestoreBindings()
    {
        BossSequenceController.RestoreBindings();
    }

    /*public static void ReportHealth(HealthManager healthManager, int baseHP, int adjustedHP, bool forceAdd = false)
    {
        if (!Instance)
        {
            return;
        }
        bool flag = false;
        if (forceAdd)
        {
            flag = true;
        }
        else
        {
            HealthManager[] array = Instance.bosses;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == healthManager)
                {
                    flag = true;
                    break;
                }
            }
        }
        if (flag)
        {
            Instance.BossHealthLookup[healthManager] = new BossHealthDetails
            {
                baseHP = baseHP,
                adjustedHP = adjustedHP
            };
        }
    }*/
}
