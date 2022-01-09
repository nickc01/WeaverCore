using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Editor.Compilation;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
    /// <summary>
    /// Used for checking the project to make sure all of WeaverCore's dependencies are met
    /// </summary>
    public static class DependencyChecker
    {
        static bool checkingDependencies = false;
        static UnboundCoroutine ProgressBarRoutine;

        static string progressTitle;
        static string progressInfo;
        static float progressProgress;

        class DependencySorter : IComparer<DependencyCheck>
        {
            Comparer<string> stringComparer = Comparer<string>.Default;
            Comparer<int> numberComparer = Comparer<int>.Default;

            public int Compare(DependencyCheck x, DependencyCheck y)
            {
                if (x.Priority == y.Priority)
                {
                    return stringComparer.Compare(x.ActionName, y.ActionName);
                }
                else
                {
                    return numberComparer.Compare(x.Priority, y.Priority);
                }
            }
        }


        [Serializable]
        class DependencyCheck_Data
        {
            public bool Silent = false;
            public int LastLeftOff = 0;
        }

        static void OnChecksCompleted(bool silent)
        {
            checkingDependencies = false;
            if (ProgressBarRoutine != null)
            {
                UnboundCoroutine.Stop(ProgressBarRoutine);
            }
            if (!silent)
            {
                DebugUtilities.ClearLog();

                Debug.Log("WeaverCore is Fully Setup!");

                //TODO - SHOW WELCOME SCREEN
            }
        }

        static void OnChecksError()
        {
            checkingDependencies = false;
            if (ProgressBarRoutine != null)
            {
                UnboundCoroutine.Stop(ProgressBarRoutine);
            }
        }


        static List<DependencyCheck> GetDependencyChecks()
        {
            List<DependencyCheck> checks = new List<DependencyCheck>();
            var types = typeof(DependencyChecker).Assembly.GetTypes().Where(t => !t.IsAbstract && !t.ContainsGenericParameters && typeof(DependencyCheck).IsAssignableFrom(t));
            foreach (var type in types)
            {
                checks.Add((DependencyCheck)Activator.CreateInstance(type));
            }
            checks.Sort(new DependencySorter());
            return checks;
        }


        static void DisplayProgressBar(string title, string info, float progress)
        {
            progressTitle = title;
            progressInfo = info;
            progressProgress = progress;
            EditorUtility.DisplayProgressBar(title, info, progress);
        }

        static void RedisplayProgressBar()
        {
            EditorUtility.DisplayProgressBar(progressTitle, progressInfo, progressProgress);
        }

        /// <summary>
        /// Checks the dependencies of a project to make sure all the WeaverCore's dependencies are satisfied
        /// </summary>
        public static void CheckDependencies()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            DependencyCheck_Data persistentData;
            if (!PersistentData.TryGetData(out persistentData))
            {
                persistentData = new DependencyCheck_Data();
                PersistentData.StoreData(persistentData);
            }



            var checks = GetDependencyChecks();

            CheckDependencies(persistentData.Silent, checks, persistentData.LastLeftOff);
        }

        static void CheckDependencies(bool silent, List<DependencyCheck> checks, int startIndex)
        {
            if (checkingDependencies)
            {
                return;
            }
            checkingDependencies = true;
            ProgressBarRoutine = UnboundCoroutine.Start(ShowProgressBar(silent));
            if (!silent)
            {
                DisplayProgressBar("Checking Dependencies", "Starting Dependency Checks", 0f);
            }
            startIndex--;

            Action<DependencyCheck.DependencyCheckResult> finishCheck = null;

            DependencyCheck currentCheck = null;

            finishCheck = result =>
            {
                try
                {
                    switch (result)
                    {
                        case DependencyCheck.DependencyCheckResult.Error:
                            if (currentCheck != null)
                            {
                                Debug.Log($"An Error occured when doing a dependency check for : {currentCheck.ActionName}. See console for more info");
                            }
                            OnChecksError();
                            return;
                        case DependencyCheck.DependencyCheckResult.RequiresReload:
                            if (!silent)
                            {
                                DebugUtilities.ClearLog();
                            }
                            PersistentData.StoreData(new DependencyCheck_Data
                            {
                                Silent = silent,
                                LastLeftOff = startIndex
                            });
                            PersistentData.SaveData();
                            return;
                    }
                    startIndex++;
                    int currentIndex = startIndex;

                    if (currentIndex >= checks.Count)
                    {
                        if (!silent)
                        {
                            EditorUtility.ClearProgressBar();
                        }
                        PersistentData.StoreData(new DependencyCheck_Data
                        {
                            Silent = true,
                            LastLeftOff = 0
                        });
                        PersistentData.SaveData();
                        OnChecksCompleted(silent);
                        return;
                    }

                    currentCheck = checks[currentIndex];
                    if (!silent)
                    {
                        DisplayProgressBar("Checking Dependencies", "Checking: " + currentCheck.ActionName, currentIndex / (float)(checks.Count - 1));
                    }
                    try
                    {
                        currentCheck.StartCheck(finishCheck);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        finishCheck(DependencyCheck.DependencyCheckResult.Error);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    OnChecksError();
                }
            };
            finishCheck(DependencyCheck.DependencyCheckResult.Complete);

        }

        static IEnumerator ShowProgressBar(bool silent)
        {
            if (silent)
            {
                yield break;
            }
            while (true)
            {
                yield return null;

                RedisplayProgressBar();
            }
        }
    }
}
