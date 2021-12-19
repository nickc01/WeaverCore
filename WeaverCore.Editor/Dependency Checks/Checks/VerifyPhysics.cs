using System;
using System.Collections;
using System.IO;
using UnityEngine;
using WeaverCore.Editor.Compilation;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
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

    class VerifyPhysics : DependencyCheck
    {
        public override void StartCheck(Action<DependencyCheckResult> finishCheck)
        {
            Physics2D.gravity = new Vector2(0f, -60f);
            finishCheck(DependencyCheckResult.Complete);
        }
    }
}
