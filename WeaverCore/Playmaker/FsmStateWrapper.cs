using System.Linq;

namespace WeaverCore.Playmaker
{
    /// <summary>
    /// Type-safe wrapper for FsmState objects
    /// </summary>
    public struct FsmStateWrapper
	{
		public object InternalState { get; }
		
		internal FsmStateWrapper(object state)
		{
			InternalState = state;
		}
		
		public FsmActionWrapper[] GetActions()
		{
			object[] actions = PlayMakerUtilities.GetActions(InternalState);
			return actions.Select(a => new FsmActionWrapper(a)).ToArray();
		}
		
		public int GetActionIndex(FsmActionWrapper action)
		{
			return PlayMakerUtilities.GetActionIndex(InternalState, action.InternalAction);
		}
		
		public FsmActionWrapper AddAction(FsmActionWrapper action)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddAction(InternalState, action.InternalAction));
		}
		
		public FsmActionWrapper AddActionAtIndex(FsmActionWrapper action, int index)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddActionAtIndex(InternalState, action.InternalAction, index));
		}
		
		public bool RemoveAction(int actionIndex)
		{
			return PlayMakerUtilities.RemoveAction(InternalState, actionIndex);
		}
		
		public FsmTransitionWrapper[] GetTransitions()
		{
			object[] transitions = PlayMakerUtilities.GetTransitions(InternalState);
			return transitions.Select(t => new FsmTransitionWrapper(t)).ToArray();
		}
		
		public FsmTransitionWrapper AddTransition(string eventName, string toState)
		{
			object transition = PlayMakerUtilities.AddTransition(InternalState, eventName, toState);
			return new FsmTransitionWrapper(transition);
		}
		
		public bool RemoveTransition(int transitionIndex)
		{
			return PlayMakerUtilities.RemoveTransition(InternalState, transitionIndex);
		}

		public override bool Equals(object obj)
		{
			if (obj is FsmStateWrapper fsm)
			{
				return fsm.InternalState == InternalState;
			}

			return false;
		}
		
		public override int GetHashCode()
		{
			return InternalState != null ? InternalState.GetHashCode() : 0;
		}

		public static bool operator==(FsmStateWrapper a, FsmStateWrapper b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(FsmStateWrapper a, FsmStateWrapper b)
		{
			return !a.Equals(b);
		}
	}
}
