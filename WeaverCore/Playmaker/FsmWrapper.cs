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
		
		internal FsmWrapper(object fsm)
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
			object state = PlayMakerUtilities.GetState(InternalFsm, stateName);
			return new FsmStateWrapper(state);
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

		public static bool operator==(FsmWrapper a, FsmWrapper b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(FsmWrapper a, FsmWrapper b)
		{
			return !a.Equals(b);
		}
	}
}
