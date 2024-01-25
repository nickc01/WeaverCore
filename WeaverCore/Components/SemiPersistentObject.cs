using Modding;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Used to keep an object despawned until the player sits on a bench
    /// </summary>
    public class SemiPersistentObject : MonoBehaviour
    {
        static HashSet<string> savedIDs = new HashSet<string>();

        //public delegate void BoolEvent(bool value);

        //public delegate void BoolRefEvent(ref bool value);

        [field: SerializeField]
        [field: Tooltip("An id to uniquely identify this object")]
        public string UniqueID { get; private set; }

        [SerializeField]
        [Tooltip("If set to true, then when this enemy gets killed, it will be automatically be marked not to spawn again until the player sits on a bench")]
        bool markOnDeath = true;

        /*private GameManager gm;

        private PlayMakerFSM myFSM;

        private bool started;

        public event BoolEvent OnSetSaveState;

        public event BoolRefEvent OnGetSaveState;*/

        //[Tooltip("An event that is called when the object retrieves it's semi-persistent state. Returns true if this object is spawnable, and false if not spawnable")]
        //public UnityEvent<bool> OnGetSaveState;

        bool started = false;

        /// <summary>
        /// If true, then this object will be spawned. If false, then this object won't spawn again until the player sits on a bench
        /// </summary>
        public bool CanBeSpawned
        {
            get => !savedIDs.Contains(UniqueID);
            set
            {
                if (value)
                {
                    savedIDs.Add(UniqueID);
                }
                else
                {
                    savedIDs.Remove(UniqueID);
                }
            }
        }

        [OnRuntimeInit]
        static void Init()
        {
            GameManager.instance.ResetSemiPersistentObjects += ResetStateStatic;
            ModHooks.SavegameLoadHook += ModHooks_SavegameLoadHook;
        }

        private static void ModHooks_SavegameLoadHook(int obj)
        {
            ResetStateStatic();
        }

        private static void ResetStateStatic()
        {
            savedIDs.Clear();
        }

        private void Reset()
        {
            UniqueID = Guid.NewGuid().ToString();
            //OnGetSaveState.RemoveAllListeners();
            //AddPersistentListener

            //typeof(UnityEventBase).GetMethod("AddPersistentListener", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(OnGetSaveState, null);
            //var registerMethod = typeof(UnityEventBase).GetMethod("RegisterPersistentListener", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int), typeof(object), typeof(MethodInfo) }, null);

            //registerMethod.Invoke(OnGetSaveState, new object[] { 0, gameObject, typeof(GameObject).GetMethod(nameof(GameObject.SetActive)) });

            //OnGetSaveState.AddListener(gameObject.SetActive);
            //OnGetSaveState.ReflectCallMethod("AddBoolPersistentListener", )
            //var addBoolMethod = typeof(UnityEventBase).GetMethod("AddBoolPersistentListener", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            //addBoolMethod.Invoke(OnGetSaveState, new object[] { new UnityAction<bool>(gameObject.SetActive),});
        }

        protected virtual void OnGetState(bool canSpawn)
        {
            GameObject.Destroy(gameObject);
        }

        //private void OnEnable()
        //{
            //gm = GameManager.instance;
            //GameManager.instance.SavePersistentObjects += SaveState;
            //GameManager.instance.ResetSemiPersistentObjects += ResetState;
            /*if (semiPersistent)
            {
                gm.ResetSemiPersistentObjects += ResetState;
            }*/
            /*if (this.OnGetSaveState == null)
            {
                LookForMyFSM();
            }*/
        //}

        //private void OnDisable()
        //{
            //GameManager.instance.SavePersistentObjects -= SaveState;
            //GameManager.instance.ResetSemiPersistentObjects -= ResetState;
        //}

        private void Start()
        {
            if (started)
            {
                return;
            }

            started = true;

            if (markOnDeath && TryGetComponent<EntityHealth>(out var entityHealth))
            {
                entityHealth.OnDeathEvent += h => CanBeSpawned = true;
            }
            //SetMyID();

            OnGetState(CanBeSpawned);

            //OnGetSaveState?.Invoke(false);
            /*PersistentBoolData persistentBoolData = SceneData.instance.FindMyState(this.persistentBoolData);
            if (persistentBoolData != null)
            {
                this.persistentBoolData.activated = persistentBoolData.activated;
                if (this.OnSetSaveState != null)
                {
                    this.OnSetSaveState(persistentBoolData.activated);
                }
                else if (myFSM != null)
                {
                    myFSM.FsmVariables.FindFsmBool("Activated").Value = persistentBoolData.activated;
                }
                else
                {
                    LookForMyFSM();
                }
            }
            else if (this.OnGetSaveState != null)
            {
                this.OnGetSaveState(ref this.persistentBoolData.activated);
            }
            else
            {
                UpdateActivatedFromFSM();
            }*/
        }

        //public void SaveState()
        //{

            /*SetMyID();
            if (this.OnGetSaveState != null)
            {
                this.OnGetSaveState(ref persistentBoolData.activated);
            }
            else
            {
                UpdateActivatedFromFSM();
            }
            SceneData.instance.SaveMyState(persistentBoolData);*/
        //}

        //private void ResetState()
        //{
            //savedIDs.Clear();
            /*if (semiPersistent)
            {
                persistentBoolData.activated = false;
                if (myFSM != null)
                {
                    myFSM.SendEvent("RESET");
                }
                else
                {
                    Debug.LogWarning("Persistent Bool Item - Couldn't reset value on FSM because it's missing.");
                }
            }*/
        //}

        /*private void SetMyID()
        {
            if (string.IsNullOrEmpty(persistentBoolData.id))
            {
                persistentBoolData.id = base.name;
            }
            if (string.IsNullOrEmpty(persistentBoolData.sceneName))
            {
                persistentBoolData.sceneName = GameManager.GetBaseSceneName(base.gameObject.scene.name);
            }
        }*/

        public void PreSetup()
        {
            Start();
        }

        /*private void UpdateActivatedFromFSM()
        {
            if (myFSM != null)
            {
                persistentBoolData.activated = myFSM.FsmVariables.FindFsmBool("Activated").Value;
            }
            else
            {
                LookForMyFSM();
            }
        }

        private void LookForMyFSM()
        {
            PlayMakerFSM[] components = GetComponents<PlayMakerFSM>();
            if (components == null)
            {
                Debug.LogErrorFormat("Persistent Bool Item ({0}) does not have a PlayMakerFSM attached to read state from.", base.name);
                return;
            }
            myFSM = FSMUtility.FindFSMWithPersistentBool(components);
            if (myFSM == null)
            {
                Debug.LogErrorFormat("Persistent Bool Item ({0}) does not have a PlayMakerFSM attached to read state from.", base.name);
            }
        }*/
    }
}