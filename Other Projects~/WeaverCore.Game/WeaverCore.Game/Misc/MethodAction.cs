using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverCore.Game
{
    /// <summary>
    /// FsmStateAction that invokes methods. This is from SFCore
    /// </summary>
    public class MethodAction : FsmStateAction
    {
        /// <summary>
        /// The method to invoke.
        /// </summary>
        public Action method;

        public string ActionName;

        /// <summary>
        /// Resets the action.
        /// </summary>
        public override void Reset()
        {
            method = null;

            base.Reset();
        }

        /// <summary>
        /// Called when the action is being processed.
        /// </summary>
        public override void OnEnter()
        {
            if (method != null) method.Invoke();
            Finish();
        }
    }
}
