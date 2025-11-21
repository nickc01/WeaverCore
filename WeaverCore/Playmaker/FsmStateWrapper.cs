using System;
using System.Linq;
using WeaverCore.Utilities;

namespace WeaverCore.Playmaker
{
    /// <summary>
    /// Type-safe wrapper for FsmState objects
    /// </summary>
    public struct FsmStateWrapper
	{
		public object InternalState { get; }
		
		public FsmStateWrapper(object state)
		{
			InternalState = state;
		}

		public string Name => InternalState.ReflectGetField<string>("name");

		public FsmWrapper GetFsm()
		{
			return PlayMakerUtilities.GetStateFsm(this);
		}
		
		public FsmActionWrapper[] GetActions()
		{
			object[] actions = PlayMakerUtilities.GetActions(InternalState);
			return actions.Select(a => new FsmActionWrapper(a)).ToArray();
		}

		public object GetActionData()
		{
			return PlayMakerUtilities.GetActionData(InternalState);
		}

		public int GetActionIndex(FsmActionWrapper action)
		{
			return PlayMakerUtilities.GetActionIndex(InternalState, action.InternalAction);
		}
		
		class QuickWeaverAction1 : WeaverFSMAction
        {
            public readonly Action MainAction;
            public QuickWeaverAction1(Action action)
            {
                MainAction = action;
            }

            public override void OnEnter()
            {
                MainAction?.Invoke();
            }
        }

        class QuickWeaverAction2 : WeaverFSMAction
        {
            public readonly Action<FsmWrapper> MainAction;
            public QuickWeaverAction2(Action<FsmWrapper> action)
            {
                MainAction = action;
            }

            public override void OnEnter()
            {
                MainAction?.Invoke(FsmWrapper);
            }
        }
		
		public FsmActionWrapper AddAction(FsmActionWrapper action)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddAction(InternalState, action.InternalAction));
		}

		public FsmActionWrapper AddActionAtIndex(FsmActionWrapper action, int index)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddActionAtIndex(InternalState, action.InternalAction, index));
		}
		


		public FsmActionWrapper AddAction(WeaverFSMAction action)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddAction(InternalState, PlayMakerUtilities.CreateFSMActionFromWeaverAction(action)));
		}

		public FsmActionWrapper AddActionAtIndex(WeaverFSMAction action, int index)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddActionAtIndex(InternalState, PlayMakerUtilities.CreateFSMActionFromWeaverAction(action), index));
		}
		



		public FsmActionWrapper AddAction(Action action)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddAction(InternalState, PlayMakerUtilities.CreateFSMActionFromWeaverAction(new QuickWeaverAction1(action))));
		}

		public FsmActionWrapper AddActionAtIndex(Action action, int index)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddActionAtIndex(InternalState, PlayMakerUtilities.CreateFSMActionFromWeaverAction(new QuickWeaverAction1(action)), index));
		}
		



		public FsmActionWrapper AddAction(Action<FsmWrapper> action)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddAction(InternalState, PlayMakerUtilities.CreateFSMActionFromWeaverAction(new QuickWeaverAction2(action))));
		}
		
		public FsmActionWrapper AddActionAtIndex(Action<FsmWrapper> action, int index)
		{
			return new FsmActionWrapper(PlayMakerUtilities.AddActionAtIndex(InternalState, PlayMakerUtilities.CreateFSMActionFromWeaverAction(new QuickWeaverAction2(action)), index));
		}
		

		public void RemoveAllActions()
		{
			PlayMakerUtilities.RemoveAllActions(InternalState);
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
