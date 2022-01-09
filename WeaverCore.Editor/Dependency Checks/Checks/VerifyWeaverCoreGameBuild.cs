using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Editor.Compilation;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
    /// <summary>
    /// Builds WeaverCore and WeaverCore.Game so that the two are in sync with each other. This makes it easy to switch back and forth between the two projects
    /// </summary>
    class VerifyWeaverCoreGameBuild : DependencyCheck
    {
        Action<DependencyCheckResult> Finish;

        public override int Priority => 100;

        public override void StartCheck(Action<DependencyCheckResult> finishCheck)
        {
            Finish = finishCheck;
            var task = BuildTools.BuildPartialWeaverCore(BuildTools.WeaverCoreBuildLocation);
            UnboundCoroutine.Start(WaitForTask(task));
        }

        IEnumerator WaitForTask(IAsyncBuildTask<BuildTools.BuildOutput> task)
        {
            yield return new WaitUntil(() => task.Completed);
            if (task.Result.Success)
            {
                Finish(DependencyCheckResult.Complete);
            }
            else
            {
                Finish(DependencyCheckResult.Error);
            }
        }
    }
}
