using System;
using UnityEngine;

public class WeaverPersistentIntItem : MonoBehaviour, ISerializationCallbackReceiver
{
    public delegate void IntEvent(int value);

    public delegate void IntRefEvent(ref int value);

    [SerializeField]
    [Tooltip("If checked, this object will reset its state under certain circumstances such as hero death.")]
    public bool semiPersistent;

    [SerializeField]
    public PersistentIntData persistentIntData;

    private GameManager gm;

    //private PlayMakerFSM myFSM;

    public event IntEvent OnSetSaveState;

    public event IntRefEvent OnGetSaveState;

    [SerializeField]
    [HideInInspector]
    string persistentIntData_id;

    [SerializeField]
    [HideInInspector]
    string persistentIntData_sceneName;

    [SerializeField]
    [HideInInspector]
    int persistentIntData_value;

    [SerializeField]
    [HideInInspector]
    bool persistentIntData_semiPersistent;

    private void Awake()
    {
        persistentIntData.semiPersistent = semiPersistent;
    }

    private void OnEnable()
    {
        gm = GameManager.instance;
        gm.SavePersistentObjects += SaveState;
        if (semiPersistent)
        {
            gm.ResetSemiPersistentObjects += ResetState;
        }
        //LookForMyFSM();
    }

    private void OnDisable()
    {
        if (gm != null)
        {
            gm.SavePersistentObjects -= SaveState;
            gm.ResetSemiPersistentObjects -= ResetState;
        }
    }

    private void Start()
    {
        SetMyID();
        PersistentIntData persistentIntData = SceneData.instance.FindMyState(this.persistentIntData);
        if (persistentIntData != null)
        {
            this.persistentIntData.value = persistentIntData.value;

            if (OnSetSaveState != null)
            {
                OnSetSaveState(persistentIntData.value);
            }

            OnPersistentValueRetrieved(persistentIntData.value);
            /*if (myFSM != null)
            {
                myFSM.FsmVariables.GetFsmInt("Value").Value = persistentIntData.value;
            }
            else
            {
                
            }*/
            //LookForMyFSM();
        }
        else if (this.OnGetSaveState != null)
        {
            this.OnGetSaveState(ref this.persistentIntData.value);
        }
        /*else
        {
            UpdateValueFromFSM();
        }*/
    }



    protected virtual void OnPersistentValueRetrieved(int persistentValue)
    {

    }

    private void Reset()
    {
        if (persistentIntData == null)
        {
            persistentIntData = new PersistentIntData();
        }
        persistentIntData.id = Guid.NewGuid().ToString();
        persistentIntData.value = -1;
    }

    private void SaveState()
    {
        SetMyID();

        if (this.OnGetSaveState != null)
        {
            this.OnGetSaveState(ref persistentIntData.value);
        }
        //UpdateValueFromFSM();
        SceneData.instance.SaveMyState(persistentIntData);
    }

    private void ResetState()
    {
        if (semiPersistent)
        {
            persistentIntData.value = -1;
            /*if (myFSM != null)
            {
                myFSM.SendEvent("RESET");
            }
            else
            {
                Debug.LogError("Persistent Bool Item - Couldn't reset value on FSM because it's missing.");
            }*/
        }
    }

    private void SetMyID()
    {
        if (string.IsNullOrEmpty(persistentIntData.id))
        {
            persistentIntData.id = base.name;
        }
        if (string.IsNullOrEmpty(persistentIntData.sceneName))
        {
            persistentIntData.sceneName = GameManager.GetBaseSceneName(base.gameObject.scene.name);
        }
    }

    /*private void UpdateValueFromFSM()
    {
        if (myFSM != null)
        {
            persistentIntData.value = myFSM.FsmVariables.GetFsmInt("Value").Value;
        }
        else
        {
            LookForMyFSM();
        }
    }*/

    /*private void SetValueOnFSM(int newValue)
    {
        if (myFSM != null)
        {
            myFSM.FsmVariables.GetFsmInt("Value").Value = newValue;
        }
    }*/

    /*private void LookForMyFSM()
    {
        PlayMakerFSM[] components = GetComponents<PlayMakerFSM>();
        if (components == null)
        {
            Debug.LogErrorFormat("Persistent Int Item ({0}) does not have a PlayMakerFSM attached to read state from.", base.name);
            return;
        }
        myFSM = FSMUtility.FindFSMWithPersistentInt(components);
        if (myFSM == null)
        {
            Debug.LogErrorFormat("Persistent Int Item ({0}) does not have a PlayMakerFSM attached to read state from.", base.name);
        }
    }*/

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
#if UNITY_EDITOR
        persistentIntData_id = persistentIntData.id;
        persistentIntData_sceneName = persistentIntData.sceneName;
        persistentIntData_value = persistentIntData.value;
        persistentIntData_semiPersistent = persistentIntData.semiPersistent;
#endif
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
#if UNITY_EDITOR
        persistentIntData = new PersistentIntData
        {
            id = persistentIntData_id,
            sceneName = persistentIntData_sceneName,
            value = persistentIntData_value,
            semiPersistent = persistentIntData_semiPersistent
        };
#endif
    }
}
