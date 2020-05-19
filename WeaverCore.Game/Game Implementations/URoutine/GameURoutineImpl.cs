using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Implementations
{
	public class GameURoutineImplementation : URoutineImplementation
    {
        static List<Data> waitingToBeStarted = new List<Data>();
        static CoroutineExecuter Executer = null;

        public override float DT => Time.deltaTime;

        IEnumerator WaiterHandle(Data data)
        {
            data.Started = true;
            try
            {
                foreach (var awaiter in new FunctionIEnumerable(data.OriginalFunction))
                {
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
                }
            }
            finally
            {
                data.Done = true;
                data.Started = false;
                waitingToBeStarted.Remove(data);
            }
        }

        public override URoutineData Start(IEnumerator<IUAwaiter> function)
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
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneCallback;
            }
            else
            {
                data.Routine = Executer.StartCoroutine(data.OriginalFunction);
            }
            return data;
        }

        private void SceneCallback(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
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
            public IEnumerator<IUAwaiter> OriginalFunction;
            public IEnumerator Function;
            public bool Started = false;
            public bool Done = false;
            public Coroutine Routine;
        }


        class FunctionIEnumerable : IEnumerable<IUAwaiter>
        {
            IEnumerator<IUAwaiter> function;

            public FunctionIEnumerable(IEnumerator<IUAwaiter> function)
            {
                this.function = function;
            }

            public IEnumerator<IUAwaiter> GetEnumerator()
            {
                return function;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

    }
}
