using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called after a PlayMakerFSM has finished initializing.
    /// The method may have no parameters or accept a PlayMakerFsmWrapper.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnPlayMakerFSMInitAttribute : PriorityAttribute
    {
        public string ObjectPath { get; }
        public string FsmName { get; }

        public OnPlayMakerFSMInitAttribute(string objectPath = "*", string fsmName = "*", int priority = 0) : base(priority)
        {
            ObjectPath = string.IsNullOrEmpty(objectPath) ? "*" : objectPath;
            FsmName = string.IsNullOrEmpty(fsmName) ? "*" : fsmName;
        }
    }
}
