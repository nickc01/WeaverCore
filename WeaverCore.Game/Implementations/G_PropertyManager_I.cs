using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;

namespace WeaverCore.Game.Implementations
{
    public class G_PropertyManager_I : WeaverCore.Implementations.PropertyManager_I
    {
        List<IPropertyTableBase> Tables = new List<IPropertyTableBase>();

        GameObject managerObject;

        public override void AddTable(IPropertyTableBase table)
        {
            Tables.Add(table);
        }

        public override void End()
        {
            if (managerObject != null)
            {
                GameObject.Destroy(managerObject);
                managerObject = null;
            }
        }

        public override void RemoveTable(IPropertyTableBase table)
        {
            Tables.Remove(table);
        }

        public override void Start()
        {
            if (managerObject == null)
            {
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
                //On.GameManager.Update += GameManager_Update;
                //On.InControl.InControlManager.Update += EarliestUpdate;
            }
        }

        /*private void GameManager_Update(On.GameManager.orig_Update orig, GameManager self)
        {
            Debugger.Log("Game Manager Called");
            On.GameManager.Update -= GameManager_Update;
        }*/

        private void SceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            //Debugger.Log("Scene Manager Called");
            if (managerObject == null)
            {
                managerObject = new GameObject("__PROPERTY_CLEANER__");
                var script = managerObject.AddComponent<CleanerScript>();
                script.Impl = this;
            }
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoaded;
        }

        class CleanerScript : MonoBehaviour
        {
            public G_PropertyManager_I Impl;

            int index = 0;

            void Start()
            {
                StartCoroutine(Cleaner());
            }

            IEnumerator Cleaner()
            {
                while (true)
                {
                    index++;
                    if (index >= Impl.Tables.Count)
                    {
                        index = 0;
                    }
                    if (Impl.Tables.Count > 0)
                    {
                        Impl.CleanTable(Impl.Tables[index]);
                    }
                    yield return new WaitForSecondsRealtime(10.0f / (Impl.Tables.Count + 1));
                }
            }
        }

    }
}
