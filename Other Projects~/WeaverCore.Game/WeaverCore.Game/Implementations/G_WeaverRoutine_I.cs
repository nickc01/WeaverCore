using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Implementations
{
	/*public class G_WeaverRoutine_I : WeaverRoutine_I
    {
        class Init : IInit
        {
            void IInit.OnInit()
            {
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneCallback;
            }
        }

        static List<Data> waitingToBeStarted = new List<Data>();
        static CoroutineExecuter Executer = null;

        public override float DT => Time.deltaTime;

        IEnumerator WaiterHandle(Data data)
        {
            data.Started = true;
            try
            {
                bool keepIterating = false;
                try
                {
                    keepIterating = data.OriginalFunction.MoveNext();
                }
                catch (Exception e)
                {
                    WeaverLog.LogError("URoutine Exception: " + e);
                    throw;
                }

                while (keepIterating)
                {
                    IWeaverAwaiter awaiter = data.OriginalFunction.Current;

                    if (awaiter == null)
                    {
                        yield return null;
                    }
                    else
                    {
                        while (awaiter.KeepWaiting())
                        {
                            yield return null;
                        }
                    }

                    try
                    {
                        keepIterating = data.OriginalFunction.MoveNext();
                    }
                    catch (Exception e)
                    {
                        WeaverLog.LogError("URoutine Exception: " + e);
                        throw;
                    }
                }
            }
            finally
            {
                data.OriginalFunction.Dispose();
                data.Done = true;
                data.Started = false;
                waitingToBeStarted.Remove(data);
            }
        }

        public override URoutineData Start(IEnumerator<IWeaverAwaiter> function)
        {
            var data = new Data()
            {
                Started = false,
                Done = false,
                OriginalFunction = function
            };

            data.Function = WaiterHandle(data);

            if (Executer == null)
            {
                waitingToBeStarted.Add(data);
            }
            else
            {
                data.Routine = Executer.StartCoroutine(data.OriginalFunction);
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneCallback;
            }
            return data;
        }

        private static void SceneCallback(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            if (Executer == null)
            {
                var gm = new GameObject("__ROUTINE_EXECUTER__");
                GameObject.DontDestroyOnLoad(gm);
                Executer = gm.AddComponent<CoroutineExecuter>();
            }
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneCallback;
        }

        public override void Stop(URoutineData routine)
        {
            var data = (Data)routine;
            if (!data.Done)
            {
                if (data.Started == false)
                {
                    waitingToBeStarted.Remove(data);
                }
                else
                {
                    Executer.StopCoroutine(data.Routine);
                    data.Started = false;
                }
            }
        }



        class CoroutineExecuter : MonoBehaviour
        {
            void Awake()
            {
                Executer = this;
                foreach (var data in waitingToBeStarted)
                {
                    data.Routine = StartCoroutine(data.Function);
                }
            }
        }


        class Data : URoutineData
        {
            public IEnumerator<IWeaverAwaiter> OriginalFunction;
            public IEnumerator Function;
            public bool Started = false;
            public bool Done = false;
            public Coroutine Routine;
        }


        class FunctionIEnumerable : IEnumerable<IWeaverAwaiter>
        {
            IEnumerator<IWeaverAwaiter> function;

            public FunctionIEnumerable(IEnumerator<IWeaverAwaiter> function)
            {
                this.function = function;
            }

            public IEnumerator<IWeaverAwaiter> GetEnumerator()
            {
                return function;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

    }*/
}
