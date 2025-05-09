using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using WeaverCore.Utilities;

namespace WeaverCore.Playmaker
{
    /// <summary>
    /// A reflection-based wrapper around the PlayMaker FsmStateAction class.
    /// </summary>
    public class WeaverFSMAction
    {
        static Type fsmActionType;
        
        // Cached property infos
        private static PropertyInfo nameProperty;
        private static PropertyInfo enabledProperty;
        private static PropertyInfo isOpenProperty;
        private static PropertyInfo activeProperty;
        private static PropertyInfo finishedProperty;
        private static PropertyInfo autoNameProperty;
        private static PropertyInfo ownerProperty;
        private static PropertyInfo fsmStateProperty;
        private static PropertyInfo fsmProperty;
        private static PropertyInfo fsmComponentProperty;
        
        // Cached method infos
        private static MethodInfo finishMethod;
        private static MethodInfo logMethod;
        private static MethodInfo logWarningMethod;
        private static MethodInfo logErrorMethod;

        public struct WeaverState
        {
            
        }

        static WeaverFSMAction()
        {
            if (PlayMakerUtilities.PlayMakerAvailable)
            {
                fsmActionType = TypeUtilities.NameToType("HutongGames.PlayMaker.FsmStateAction", PlayMakerUtilities.PlayMakerFSMType.Assembly.GetName().Name);
                
                if (fsmActionType != null)
                {
                    // Cache property infos
                    nameProperty = fsmActionType.GetProperty("Name");
                    enabledProperty = fsmActionType.GetProperty("Enabled");
                    isOpenProperty = fsmActionType.GetProperty("IsOpen");
                    activeProperty = fsmActionType.GetProperty("Active");
                    finishedProperty = fsmActionType.GetProperty("Finished");
                    autoNameProperty = fsmActionType.GetProperty("AutoName");
                    ownerProperty = fsmActionType.GetProperty("Owner");
                    fsmStateProperty = fsmActionType.GetProperty("FsmState");
                    fsmProperty = fsmActionType.GetProperty("Fsm");
                    fsmComponentProperty = fsmActionType.GetProperty("FsmComponent");
                    
                    // Cache method infos
                    finishMethod = fsmActionType.GetMethod("Finish");
                    logMethod = fsmActionType.GetMethod("Log");
                    logWarningMethod = fsmActionType.GetMethod("LogWarning");
                    logErrorMethod = fsmActionType.GetMethod("LogError");
                }
            }
        }

        public object FSMActionBase { get; set; }

        public string Name
        {
            get => FSMActionBase != null && nameProperty != null ? nameProperty.GetValue(FSMActionBase) as string : null;
            set
            {
                if (FSMActionBase != null && nameProperty != null)
                {
                    nameProperty.SetValue(FSMActionBase, value);
                }
            }
        }

        public bool Enabled
        {
            get => FSMActionBase != null && enabledProperty != null ? (bool)(enabledProperty.GetValue(FSMActionBase) ?? true) : true;
            set
            {
                if (FSMActionBase != null && enabledProperty != null)
                {
                    enabledProperty.SetValue(FSMActionBase, value);
                }
            }
        }

        public bool IsOpen
        {
            get => FSMActionBase != null && isOpenProperty != null ? (bool)(isOpenProperty.GetValue(FSMActionBase) ?? true) : true;
            set
            {
                if (FSMActionBase != null && isOpenProperty != null)
                {
                    isOpenProperty.SetValue(FSMActionBase, value);
                }
            }
        }

        public bool Active
        {
            get => FSMActionBase != null && activeProperty != null ? (bool)(activeProperty.GetValue(FSMActionBase) ?? false) : false;
            set
            {
                if (FSMActionBase != null && activeProperty != null)
                {
                    activeProperty.SetValue(FSMActionBase, value);
                }
            }
        }

        public bool Finished
        {
            get => FSMActionBase != null && finishedProperty != null ? (bool)(finishedProperty.GetValue(FSMActionBase) ?? false) : false;
            set
            {
                if (FSMActionBase != null && finishedProperty != null)
                {
                    finishedProperty.SetValue(FSMActionBase, value);
                }
            }
        }

        public bool AutoName
        {
            get => FSMActionBase != null && autoNameProperty != null ? (bool)(autoNameProperty.GetValue(FSMActionBase) ?? false) : false;
            set
            {
                if (FSMActionBase != null && autoNameProperty != null)
                {
                    autoNameProperty.SetValue(FSMActionBase, value);
                }
            }
        }

        public GameObject Owner
        {
            get => FSMActionBase != null && ownerProperty != null ? ownerProperty.GetValue(FSMActionBase) as GameObject : null;
        }

        public object FsmState
        {
            get => FSMActionBase != null && fsmStateProperty != null ? fsmStateProperty.GetValue(FSMActionBase) : null;
        }

        public object Fsm
        {
            get => FSMActionBase != null && fsmProperty != null ? fsmProperty.GetValue(FSMActionBase) : null;
        }

        public MonoBehaviour FsmComponent
        {
            get => FSMActionBase != null && fsmComponentProperty != null ? fsmComponentProperty.GetValue(FSMActionBase) as MonoBehaviour : null;
        }



        public virtual void Init(object state) { }
        public virtual void Reset() { }
        public virtual void OnPreprocess() { }
        public virtual void Awake() { }
        public virtual bool Event(WeaverFsmEvent fsmEvent) { return false; }
        public virtual bool Event(object fsmEvent) 
        { 
            if (fsmEvent != null)
            {
                return Event(new WeaverFsmEvent(fsmEvent));
            }
            return false;
        }
        public virtual void OnEnter() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnUpdate() { }
        public virtual void OnGUI() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnExit() { }

        public virtual void DoCollisionEnter(Collision collisionInfo) { }
        public virtual void DoCollisionStay(Collision collisionInfo) { }
        public virtual void DoCollisionExit(Collision collisionInfo) { }
        public virtual void DoTriggerEnter(Collider other) { }
        public virtual void DoTriggerStay(Collider other) { }
        public virtual void DoTriggerExit(Collider other) { }
        public virtual void DoParticleCollision(GameObject other) { }
        public virtual void DoCollisionEnter2D(Collision2D collisionInfo) { }
        public virtual void DoCollisionStay2D(Collision2D collisionInfo) { }
        public virtual void DoCollisionExit2D(Collision2D collisionInfo) { }
        public virtual void DoTriggerEnter2D(Collider2D other) { }
        public virtual void DoTriggerStay2D(Collider2D other) { }
        public virtual void DoTriggerExit2D(Collider2D other) { }
        public virtual void DoControllerColliderHit(ControllerColliderHit collider) { }
        public virtual void DoJointBreak(float force) { }
        public virtual void DoJointBreak2D(Joint2D joint) { }
        public virtual void DoAnimatorMove() { }
        public virtual void DoAnimatorIK(int layerIndex) { }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            MonoBehaviour component = FsmComponent;
            if (component != null)
            {
                return component.StartCoroutine("DoCoroutine", routine);
            }
            return null;
        }

        public void StopCoroutine(Coroutine routine)
        {
            MonoBehaviour component = FsmComponent;
            if (component != null && routine != null)
            {
                component.StopCoroutine(routine);
            }
        }

        // Utility methods
        public void Finish()
        {
            // Forward to the impl class if it exists
            if (FSMActionBase != null && finishMethod != null)
            {
                finishMethod.Invoke(FSMActionBase, null);
            }
        }

        public void Log(string text)
        {
            if (FSMActionBase != null && logMethod != null)
            {
                logMethod.Invoke(FSMActionBase, new object[] { text });
            }
        }

        public void LogWarning(string text)
        {
            if (FSMActionBase != null && logWarningMethod != null)
            {
                logWarningMethod.Invoke(FSMActionBase, new object[] { text });
            }
        }

        public void LogError(string text)
        {
            if (FSMActionBase != null && logErrorMethod != null)
            {
                logErrorMethod.Invoke(FSMActionBase, new object[] { text });
            }
        }
    }
}