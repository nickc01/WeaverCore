using System;
using System.Linq;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Playmaker
{
    /// <summary>
    /// Type-safe wrapper for Fsm objects
    /// </summary>
    public struct FsmWrapper
    {
        public object InternalFsm { get; }

        public FsmWrapper(object fsm)
        {
            InternalFsm = fsm;
        }

        public FsmStateWrapper[] GetStates()
        {
            object[] states = PlayMakerUtilities.GetStates(InternalFsm);
            return states.Select(s => new FsmStateWrapper(s)).ToArray();
        }

        public FsmStateWrapper GetState(string stateName)
        {
            //WeaverLog.Log($"GET State - Internal FSM = {(InternalFsm?.GetType().Name ?? "null")}");
            //WeaverLog.Log($"GET State - State Name = {(stateName ?? "null")}");
            object state = PlayMakerUtilities.GetState(InternalFsm, stateName);
            //WeaverLog.Log($"GET State - State Value = {(state?.GetType().Name ?? "null")}");
            return new FsmStateWrapper(state);
        }

        public bool TryGetState(string stateName, out FsmStateWrapper state)
        {
            return (state = GetState(stateName)) != default;
        }


        public FsmStateWrapper AddState(string stateName, Vector2 position)
        {
            object state = PlayMakerUtilities.AddState(InternalFsm, stateName, position);
            return new FsmStateWrapper(state);
        }

        public bool RemoveState(string stateName)
        {
            return PlayMakerUtilities.RemoveState(InternalFsm, stateName);
        }

        public FsmStateWrapper GetActiveState()
        {
            object state = PlayMakerUtilities.GetActiveState(InternalFsm);
            return new FsmStateWrapper(state);
        }

        public string GetActiveStateName()
        {
            return PlayMakerUtilities.GetActiveStateName(InternalFsm);
        }

        public void SetActiveState(string stateName)
        {
            PlayMakerUtilities.SetActiveState(InternalFsm, stateName);
        }

        public FsmEventWrapper[] GetEvents()
        {
            object[] events = PlayMakerUtilities.GetEvents(InternalFsm);
            return events.Select(e => new FsmEventWrapper(e)).ToArray();
        }

        public FsmEventWrapper AddEvent(string eventName)
        {
            object evt = PlayMakerUtilities.AddEvent(InternalFsm, eventName);
            return new FsmEventWrapper(evt);
        }

        public void SendEvent(string eventName)
        {
            PlayMakerUtilities.SendEvent(InternalFsm, eventName);
        }

        public FsmTransitionWrapper[] GetGlobalTransitions()
        {
            object[] transitions = PlayMakerUtilities.GetGlobalTransitions(InternalFsm);
            return transitions.Select(t => new FsmTransitionWrapper(t)).ToArray();
        }

        public FsmTransitionWrapper AddGlobalTransition(string eventName, string toState)
        {
            object transition = PlayMakerUtilities.AddGlobalTransition(InternalFsm, eventName, toState);
            return new FsmTransitionWrapper(transition);
        }

        public bool RemoveGlobalTransition(int transitionIndex)
        {
            return PlayMakerUtilities.RemoveGlobalTransition(InternalFsm, transitionIndex);
        }

        public FsmVariablesWrapper GetVariables()
        {
            object variables = PlayMakerUtilities.GetVariables(InternalFsm);
            return new FsmVariablesWrapper(variables);
        }

        // Variable access methods
        public bool GetBoolVariable(string variableName)
        {
            return PlayMakerUtilities.GetBoolVariable(InternalFsm, variableName);
        }

        public void SetBoolVariable(string variableName, bool value)
        {
            PlayMakerUtilities.SetBoolVariable(InternalFsm, variableName, value);
        }

        public int GetIntVariable(string variableName)
        {
            return PlayMakerUtilities.GetIntVariable(InternalFsm, variableName);
        }

        public void SetIntVariable(string variableName, int value)
        {
            PlayMakerUtilities.SetIntVariable(InternalFsm, variableName, value);
        }

        public float GetFloatVariable(string variableName)
        {
            return PlayMakerUtilities.GetFloatVariable(InternalFsm, variableName);
        }

        public void SetFloatVariable(string variableName, float value)
        {
            PlayMakerUtilities.SetFloatVariable(InternalFsm, variableName, value);
        }

        public string GetStringVariable(string variableName)
        {
            return PlayMakerUtilities.GetStringVariable(InternalFsm, variableName);
        }

        public void SetStringVariable(string variableName, string value)
        {
            PlayMakerUtilities.SetStringVariable(InternalFsm, variableName, value);
        }

        public Vector3 GetVector3Variable(string variableName)
        {
            return PlayMakerUtilities.GetVector3Variable(InternalFsm, variableName);
        }

        public void SetVector3Variable(string variableName, Vector3 value)
        {
            PlayMakerUtilities.SetVector3Variable(InternalFsm, variableName, value);
        }

        public GameObject GetGameObjectVariable(string variableName)
        {
            return PlayMakerUtilities.GetGameObjectVariable(InternalFsm, variableName);
        }

        public void SetGameObjectVariable(string variableName, GameObject value)
        {
            PlayMakerUtilities.SetGameObjectVariable(InternalFsm, variableName, value);
        }

        public UnityEngine.Object GetObjectVariable(string variableName)
        {
            return PlayMakerUtilities.GetObjectVariable(InternalFsm, variableName);
        }

        public void SetObjectVariable(string variableName, UnityEngine.Object value)
        {
            PlayMakerUtilities.SetObjectVariable(InternalFsm, variableName, value);
        }


        public Color GetColorVariable(string variableName)
        {
            return PlayMakerUtilities.GetColorVariable(InternalFsm, variableName);
        }

        public void SetColorVariable(string variableName, Color value)
        {
            PlayMakerUtilities.SetColorVariable(InternalFsm, variableName, value);
        }


        public Enum GetEnumVariable(string variableName)
        {
            return PlayMakerUtilities.GetEnumVariable(InternalFsm, variableName);
        }

        public void SetEnumVariable(string variableName, Enum value)
        {
            PlayMakerUtilities.SetEnumVariable(InternalFsm, variableName, value);
        }


        public object[] GetArrayVariable(string variableName)
        {
            return PlayMakerUtilities.GetArrayVariable(InternalFsm, variableName);
        }

        public void SetArrayVariable(string variableName, object[] value)
        {
            PlayMakerUtilities.SetArrayVariable(InternalFsm, variableName, value);
        }

		public Material GetMaterialVariable(string variableName)
		{
			return PlayMakerUtilities.GetMaterialVariable(InternalFsm, variableName);
		}

		public void SetMaterialVariable(string variableName, Material value)
        {
            PlayMakerUtilities.SetMaterialVariable(InternalFsm, variableName, value);
        }

		public Vector2 GetVector2Variable(string variableName)
		{
			return PlayMakerUtilities.GetVector2Variable(InternalFsm, variableName);
		}

		public void SetVector2Variable(string variableName, Vector2 value)
        {
            PlayMakerUtilities.SetVector2Variable(InternalFsm, variableName, value);
        }

		public Rect GetRectVariable(string variableName)
		{
			return PlayMakerUtilities.GetRectVariable(InternalFsm, variableName);
		}

		public void SetRectVariable(string variableName, Rect value)
        {
            PlayMakerUtilities.SetRectVariable(InternalFsm, variableName, value);
        }

		public Quaternion GetQuaternionVariable(string variableName)
		{
			return PlayMakerUtilities.GetQuaternionVariable(InternalFsm, variableName);
		}

		public void SetQuaternionVariable(string variableName, Quaternion value)
        {
            PlayMakerUtilities.SetQuaternionVariable(InternalFsm, variableName, value);
        }

		public Texture GetTextureVariable(string variableName)
		{
			return PlayMakerUtilities.GetTextureVariable(InternalFsm, variableName);
		}

		public void SetTextureVariable(string variableName, Texture value)
        {
            PlayMakerUtilities.SetTextureVariable(InternalFsm, variableName, value);
        }

        public override bool Equals(object obj)
        {
            if (obj is FsmWrapper fsm)
            {
                return fsm.InternalFsm == InternalFsm;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return InternalFsm != null ? InternalFsm.GetHashCode() : 0;
        }

        public static bool operator ==(FsmWrapper a, FsmWrapper b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(FsmWrapper a, FsmWrapper b)
        {
            return !a.Equals(b);
        }
    }
}
