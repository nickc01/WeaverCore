using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{

    public class WeaverBossSceneController : BossSceneController
    {
        static MethodInfo ReportHealthMethod;
        static MethodInfo CheckBossesDeadMethod;
        static Func<BossSceneController, int> getBossesLeft;
        static Action<BossSceneController, int> setBossesLeft;

        static Type HealthManagerType = null;
        static EventInfo OnDeathEventHM;

        [SerializeField]
        [Tooltip("If scene end is handled elsewhere then leave empty. Only assign bosses here if you want the scene to end on EntityHealth death event.")]
        EntityHealth[] weaverBosses;

        [OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            var awake = typeof(BossSceneController).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            var awake_prefix = typeof(WeaverBossSceneController).GetMethod(nameof(Awake_Prefix), BindingFlags.Static | BindingFlags.NonPublic);

            patcher.Patch(awake, awake_prefix, null);


            var setup = typeof(BossSceneController).GetMethod("Setup", BindingFlags.NonPublic | BindingFlags.Instance);
            var setup_prefix = typeof(WeaverBossSceneController).GetMethod(nameof(Setup_Prefix), BindingFlags.Static | BindingFlags.NonPublic);

            patcher.Patch(setup, setup_prefix, null);

            getBossesLeft = ReflectionUtilities.CreateFieldGetter<BossSceneController, int>("bossesLeft");
            setBossesLeft = ReflectionUtilities.CreateFieldSetter<BossSceneController, int>("bossesLeft");

            CheckBossesDeadMethod = typeof(BossSceneController).GetMethod("CheckBossesDead", BindingFlags.NonPublic | BindingFlags.Instance);

            if (Initialization.Environment == Enums.RunningState.Game)
            {
                HealthManagerType = typeof(CameraController).Assembly.GetType("HealthManager");

                OnDeathEventHM = HealthManagerType.GetEvent("OnDeath");
            }

            var doDreamReturnMethod = typeof(BossSceneController).GetMethod(nameof(DoDreamReturn));

            var doDreamReturn_Postfix = typeof(WeaverBossSceneController).GetMethod(nameof(DoDreamReturn_Postfix),BindingFlags.Static | BindingFlags.NonPublic);

            patcher.Patch(doDreamReturnMethod,null, doDreamReturn_Postfix);
        }

        static MonoBehaviour GetHealthManager(GameObject gameObject)
        {
            if (HealthManagerType != null)
            {
                return gameObject.GetComponent(HealthManagerType) as MonoBehaviour;
            }
            else
            {
                return null;
            }
        }

        static MonoBehaviour GetHealthManager(Component component)
        {
            if (HealthManagerType != null && component != null)
            {
                return component.GetComponent(HealthManagerType) as MonoBehaviour;
            }
            else
            {
                return null;
            }
        }

        static void DoDreamReturn_Postfix(BossSceneController __instance)
        {
            //WeaverLog.Log("DOING SCENE TRANSITION");
            var transition = GameObject.FindObjectOfType<WeaverBossSceneTransition>();

            if (transition != null)
            {
                transition.BeginDreamReturn();
                //EventManager.SendEventToGameObject(__instance.DreamReturnEvent, transition.gameObject);
            }
        }

        static bool Awake_Prefix(BossSceneController __instance)
        {
            if (__instance is WeaverBossSceneController wbsc)
            {
                wbsc.transitionPrefab = GG_Internal.ggBattleTransitions;

                if (Initialization.Environment == Enums.RunningState.Game)
                {
                    var bosses = typeof(BossSceneController).GetField("bosses");

                    if (bosses.GetValue(wbsc) == null)
                    {
                        bosses.SetValue(wbsc,Array.CreateInstance(HealthManagerType,0));
                    }
                }

                //WeaverLog.Log("SETUP EVENT IS NULL = " + (SetupEvent == null));
            }
            return true;
        }

        static bool Setup_Prefix(BossSceneController __instance)
        {
            if (__instance is WeaverBossSceneController wbsc)
            {
                //WeaverLog.Log("SETUP FUNC");
                foreach (var boss in wbsc.weaverBosses)
                {
                    var healthManager = GetHealthManager(boss);

                    if (healthManager != null)
                    {
                        wbsc.AddBossInternal(healthManager);
                    }
                    else
                    {
                        wbsc.AddBossInternal(boss);
                    }
                }
            }
            return true;
        }

        public static void AddBoss(EntityHealth bossHealth)
        {
            AddBoss((Component)bossHealth);
        }

        public static void AddBoss(GameObject bossObject)
        {
            if (bossObject.TryGetComponent<EntityHealth>(out var entityHealth))
            {
                AddBoss(entityHealth);
            }
            else
            {
                var healthManager = GetHealthManager(bossObject);
                if (healthManager != null)
                {
                    AddBoss(healthManager);
                }
            }
        }

        public static void AddBoss(Component component)
        {
            if (Instance != null)
            {
                var healthManager = GetHealthManager(component);

                if (healthManager != null)
                {
                    setBossesLeft(Instance, getBossesLeft(Instance) + 1);

                    Action onDeath = null;

                    onDeath = () =>
                    {
                        setBossesLeft(Instance, getBossesLeft(Instance) - 1);
                        //entityHealth.OnDeathEvent -= onDeath;
                        OnDeathEventHM.RemoveEventHandler(healthManager, onDeath);
                        CheckBossesDeadMethod.Invoke(Instance, null);
                    };

                    OnDeathEventHM.AddEventHandler(healthManager, onDeath);
                }
                else if (component is EntityHealth entityHealth && Instance is WeaverBossSceneController wbsc)
                {
                    setBossesLeft(wbsc, getBossesLeft(wbsc) + 1);

                    Action<HitInfo> onDeath = null;

                    onDeath = hitInfo =>
                    {
                        setBossesLeft(wbsc, getBossesLeft(wbsc) - 1);
                        entityHealth.OnDeathEvent -= onDeath;
                        CheckBossesDeadMethod.Invoke(wbsc, null);
                    };

                    entityHealth.OnDeathEvent += onDeath;
                }
            }
        }

        void AddBossInternal(Component component)
        {
            if (component is EntityHealth healthComponent)
            {
                AddBossInternal(healthComponent);
            }
            else if (HealthManagerType != null && HealthManagerType.IsAssignableFrom(component.GetType()))
            {
                setBossesLeft(this, getBossesLeft(this) + 1);

                Action onDeath = null;

                onDeath = () =>
                {
                    setBossesLeft(this, getBossesLeft(this) - 1);
                    //entityHealth.OnDeathEvent -= onDeath;
                    OnDeathEventHM.RemoveEventHandler(component, onDeath);
                    CheckBossesDeadMethod.Invoke(this, null);
                };

                OnDeathEventHM.AddEventHandler(component, onDeath);
                //entityHealth.OnDeathEvent += onDeath;
            }
        }

        void AddBossInternal(EntityHealth entityHealth)
        {
            if (entityHealth != null)
            {
                setBossesLeft(this, getBossesLeft(this) + 1);

                Action<HitInfo> onDeath = null;

                onDeath = hitInfo =>
                {
                    setBossesLeft(this, getBossesLeft(this) - 1);
                    entityHealth.OnDeathEvent -= onDeath;
                    CheckBossesDeadMethod.Invoke(this, null);
                };

                entityHealth.OnDeathEvent += onDeath;
            }
        }

        Dictionary<EntityHealth, BossHealthDetails> weaverBossHealthLookup;


        public static void ReportHealth(EntityHealth entityHealth, int baseHP, int adjustedHP, bool forceAdd = false)
        {
            if (Instance == null)
            {
                return;
            }

            var healthManager = GetHealthManager(entityHealth);

            if (healthManager != null)
            {
                if (ReportHealthMethod == null)
                {
                    ReportHealthMethod = typeof(BossSceneController).GetMethod("ReportHealth", BindingFlags.Public | BindingFlags.Static);
                }

                if (ReportHealthMethod != null)
                {
                    ReportHealthMethod.Invoke(null, new object[] { healthManager, baseHP, adjustedHP, forceAdd });
                    return;
                }
            }

            if (Instance is WeaverBossSceneController wbsc)
            {
                bool flag = false;
                if (forceAdd)
                {
                    flag = true;
                }
                else
                {
                    //HealthManager[] array = Instance.bosses;
                    EntityHealth[] bosses = wbsc.weaverBosses;

                    if (bosses != null)
                    {
                        for (int i = 0; i < bosses.Length; i++)
                        {
                            if (bosses[i] == entityHealth)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                if (flag)
                {
                    wbsc.weaverBossHealthLookup[entityHealth] = new BossHealthDetails
                    {
                        baseHP = baseHP,
                        adjustedHP = adjustedHP
                    };
                }
            }
        }
    }
}
