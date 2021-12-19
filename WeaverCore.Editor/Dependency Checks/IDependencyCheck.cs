﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{

    public abstract class DependencyCheck
    {
        public enum DependencyCheckResult
        {
            Error,
            RequiresReload,
            Complete
        }

        public virtual int Priority => 0;
        public virtual string ActionName => StringUtilities.Prettify(GetType().Name);


        /// <summary>
        /// Starts doing the dependency check. Be sure to call <paramref name="finishCheck"/> delegate to finish the check
        /// </summary>
        public abstract void StartCheck(Action<DependencyCheckResult> finishCheck);
    }

    //public interface IDependencyCheck
    //{
        

        /*int Priority { get; }
        string ActionName { get; }

        /// <summary>
        /// A coroutine for checking a dependency. Use "yield return null" to wait a frame, and yield a <see cref="DependencyCheckResult"/> value to end the routine
        /// </summary>
        IEnumerable<DependencyCheckResult?> CheckRoutine();*/
    //}
}
