using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoidCore;
using VoidCore.Debug;
using VoidCore.Hooks;

internal class GameObjectTracker : MonoBehaviour
{
    Coroutine slowUpdate;


    internal static GameObject Tracker = null;
    internal static HashSet<GameObject> AllGameObjects = new HashSet<GameObject>();

    public static IEnumerable<GameObject> LoadedGameObjects => AllGameObjects;

    static bool Ending = false;

    internal static void StartTracker()
    {
        if (Tracker == null)
        {
            Tracker = new GameObject("__GAMEOBJECTTRACKER__");
            DontDestroyOnLoad(Tracker);
            Tracker.AddComponent<GameObjectTracker>();
        }
    }

    internal static void RemoveGameObject(GameObject g)
    {
        if (Ending == false)
        {
            AllGameObjects.Remove(g);
        }
    }

    internal static void StopTracker()
    {
        Ending = true;
        //Events.InternalOnDebugEnd?.Invoke();
        foreach (var g in AllGameObjects)
        {
            var voidComponent = g.GetComponent<VoidMonitor>();
            if (voidComponent != null)
            {
                Destroy(voidComponent);
            }
        }
        AllGameObjects.Clear();
        Ending = false;
    }

    void Start()
    {
        slowUpdate = StartCoroutine(SlowUpdate());
    }

    IEnumerator SlowUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            foreach (var g in FindObjectsOfType<GameObject>())
            {
                if (g != gameObject && AllGameObjects.Add(g))
                {
                    g.AddComponent<VoidMonitor>();
                }
            }
        }
    }
}
